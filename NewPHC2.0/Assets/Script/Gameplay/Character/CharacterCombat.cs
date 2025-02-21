using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class CharacterCombat : MonoBehaviour, ICombatEntity
{
    private class MaterialData
    {
        public SpriteRenderer renderer { get; set; }
        public Material material { get; set; }
        public Color color { get; set; }
    }

    public float MaxHealth { get => _maxHealth; }
    public float Health { get => _health; }
    public float Speed { get => _speed; set => _speed = value; }
    [SerializeField] protected float _maxHealth = 100;
    [SerializeField] protected float _health = 100;
    [SerializeField] protected float _speed = 1;
    private float multiplySpeed = 1;

    public Vector3 Offset { get => _offset; }
    [SerializeField] protected Vector3 _offset = Vector3.zero;

    protected bool died = false;

    protected bool canRotate = true;
    protected bool canWalk = true;
    private float nowForcePower = 0;

    protected bool changingMaterial = false;

    public Collider2D Collider { get => _collider; }
    protected Collider2D _collider;
    protected Rigidbody2D _rigidbody;
    protected Animator _animator;

    protected virtual void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(transform.position + _offset, 0.05f);
    }

    public void Heal(float heal)
    {
        if (_health <= 0) return;

        _health = Mathf.Min(_health + heal, _maxHealth);
    }

    public virtual void TakeDamage(float dmg)
    {
        _health = Mathf.Max(_health - dmg, 0);

        HurtMaterial();

        if (_health <= 0)
        {
            Die();
        }
    }

    private void HurtMaterial() => StartCoroutine(HurtMaterialIE());

    private IEnumerator HurtMaterialIE()
    {
        if (changingMaterial) yield break;

        var hurtMaterial = Resources.Load<Material>("Material/HurtMaterial");

        yield return ChangeMaterialIE(hurtMaterial, Color.white, 0.15f, 0.1f);
    }

    public void SkipAndChangeMaterial(Material material, Color color, float changeTime, float nextChangeDelay)
    {
        changingMaterial = false;
        StartCoroutine(ChangeMaterialIE(material, color, changeTime, nextChangeDelay));
    }

    public void ChangeMaterial(Material material, Color color, float changeTime, float nextChangeDelay) =>
        StartCoroutine(ChangeMaterialIE(material, color, changeTime, nextChangeDelay));

    public IEnumerator ChangeMaterialIE(Material material, Color color, float changeTime, float nextChangeDelay)
    {
        if (changingMaterial) yield break;

        changingMaterial = true;

        var materialDatas = new List<MaterialData>();

        void AddMaterialData(Transform parent)
        {
            if (parent.TryGetComponent(out SpriteRenderer renderer))
            {
                if (renderer.sprite != null)
                    materialDatas.Add(new MaterialData
                    {
                        renderer = renderer,
                        material = renderer.material,
                        color = renderer.color
                    });
            }

            foreach (Transform child in parent)
                AddMaterialData(child);
        }

        AddMaterialData(transform);

        foreach (var materialData in materialDatas)
        {
            if (materialData.renderer != null)
            {
                materialData.renderer.material = material;
                materialData.renderer.color = color;
            }
        }

        yield return new WaitForSeconds(changeTime);

        foreach (var materialData in materialDatas)
        {
            if (materialData.renderer != null)
            {
                materialData.renderer.material = materialData.material;
                materialData.renderer.color = materialData.color;
            }
        }

        yield return new WaitForSeconds(nextChangeDelay);

        changingMaterial = false;
    }

    public void ApplyForce(Vector2 vector, float power) => StartCoroutine(ApplyForceIE(vector, power));

    protected virtual IEnumerator ApplyForceIE(Vector2 vector, float power)
    {
        if (power <= nowForcePower || died) yield break;

        canWalk = false;

        nowForcePower = power;

        if (_animator != null)
        {
            _animator.SetFloat("RunState", 1);
            _animator.SetBool("Run", true);
        }

        _rigidbody.AddRelativeForce(vector * power * 15, ForceMode2D.Impulse);

        for (int i = 0; i < 10; i++)
        {
            if (nowForcePower != power)
                yield break;

            _rigidbody.velocity *= 0.725f;

            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.05f);

        if (_animator != null)
            _animator.SetFloat("RunState", 0);

        _rigidbody.velocity = Vector2.zero;

        canWalk = true;
        nowForcePower = 0;
    }

    public T AddStatusEffect<T>() where T : StatusEffect
    {
        StatusEffect[] allStatus = GetComponents<StatusEffect>().Where(s => !s.CanStack).ToArray();
        T status = gameObject.AddComponent<T>();

        if (allStatus.Length > 0 && !status.CanStack)
        {
            Destroy(status);
            return null;
        }

        return status;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public virtual void Die()
    {
        if (_health > 0) return;

        died = true;

        Destroy(gameObject);
    }

    public void MultiplySpeed(float speed)
    {
        multiplySpeed *= speed;
    }

    protected void Attack()
    {
        if (died) return;

        StartCoroutine(AttackIE());
    }

    protected abstract IEnumerator AttackIE();

    protected virtual void Move(Vector2 vector)
    {
        if (!canWalk || died) return;

        var velocity = vector.normalized * _speed * multiplySpeed * 3.5f;
        _rigidbody.velocity = velocity;

        SetAnimator(velocity);
    }

    protected virtual void SetAnimator(Vector2 velocity)
    {
        if (_animator == null) return;

        if (velocity.magnitude > 0)
        {
            _animator.SetBool("Run", true);
            _animator.SetFloat("RunState", Mathf.Min(_animator.GetFloat("RunState") + (Time.deltaTime * 5), 0.5f));
        }
        else
        {
            _animator.SetBool("Run", false);
            _animator.SetFloat("RunState", Mathf.Max(_animator.GetFloat("RunState") - (Time.deltaTime * 5), 0));
        }

        if (canRotate)
        {
            var oldRotation = transform.rotation;

            if (velocity.x > 0)
                transform.rotation = Quaternion.Euler(oldRotation.x, 180, oldRotation.z);
            else if (velocity.x < 0)
                transform.rotation = Quaternion.Euler(oldRotation.x, 0, oldRotation.z);
        }
    }
}
