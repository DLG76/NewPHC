using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VoidObject : MonoBehaviour
{
    public VoidItem Item { get => _item; }
    private VoidItem _item;

    private List<ICombatEntity> _entityAttacked = new List<ICombatEntity>();
    public UnityAction<Vector2> onItemStart;
    public UnityAction<ICombatEntity> onItemHitEnemy;
    public UnityAction<VoidItemDrop> onItemDestroy;

    [SerializeField] private Rigidbody2D _rigidbody;

    private bool attacked = false;
    private Vector2 nowVelocity = Vector2.zero;

    public float AttackDamage
    {
        get => damageOnCharge * _damageMultiply;
    }

    public float SpeedMultiply
    {
        get => _speedMultiply;
        set
        {
            if (!attacked)
                _speedMultiply = value;
        }
    }
    private float _speedMultiply = 1;

    public float DamageMultiply
    {
        get => _damageMultiply;
        set
        {
            if (!attacked)
                _damageMultiply = value;
        }
    }
    private float _damageMultiply = 1;

    public bool Hitted { get; private set; }
    public Vector2 HitPoint { get; private set; }
    public float HitDistance { get; private set; }

    private float speedOnCharge;
    private float damageOnCharge;
    private float chargeTime = 0;
    private float effectTime = 0;
    private bool rotatingRight = true;
    private bool lockMoveVoid = false;
    private Vector3 moveOffset = Vector3.zero;

    private ParticleSystem dustEffect;
    private Transform voidTransform;
    private Transform chargeCanvas;
    private Slider chargeBar;
    private Image chargeBarImage;

    private void Awake()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
            _rigidbody = gameObject.AddComponent<Rigidbody2D>();

        _rigidbody.freezeRotation = false;
        _rigidbody.isKinematic = false;
        _rigidbody.gravityScale = 0;
        _rigidbody.drag = 0;

        tag = "Void";
        gameObject.layer = LayerMask.NameToLayer("Void");
    }

    private void Update()
    {
        if (_item.Owner != null && _item.Owner.Character != null && !attacked)
        {
            var plrPos = _item.Owner.Character.transform.position + _item.Owner.Character.Offset;
            plrPos += _item.Offset.ConvertTo<Vector3>() + moveOffset;

            transform.position = Vector3.Lerp(transform.position, plrPos, Time.deltaTime * (lockMoveVoid ? 60 : 12));
            //transform.position = plrPos;
        }
        else if (attacked)
        {
            _rigidbody.velocity = nowVelocity;
        }
    }

    public void Setup(VoidItem item)
    {
        _item = item;
        
        if (item.voidPrefab != null)
            voidTransform = Instantiate(item.voidPrefab, transform.position, transform.rotation, transform);

        chargeCanvas = Instantiate(Resources.Load<Transform>("UI/ChargeCanvas"), transform.position + Vector3.up, Quaternion.identity, transform);
        chargeBar = chargeCanvas.GetComponentInChildren<Slider>();
        chargeBar.value = 0;
        chargeBar.maxValue = 1;
        chargeBarImage = chargeBar.transform.Find("Fill Area/Fill").GetComponent<Image>();

        foreach (var skillItem in _item.SkillItems)
            skillItem.Setup(this);

        transform.position = _item.Owner.Character.transform.position;

        speedOnCharge = item.MinSpeed;
        damageOnCharge = item.MinDamage;
    }

    public void Charge()
    {
        if (this == null || gameObject == null || transform == null || voidTransform == null) return;

        chargeTime = Mathf.Min(chargeTime + Time.deltaTime, _item.ChargeTime);

        float lerpTime = chargeTime / _item.ChargeTime;

        if (chargeBar != null && chargeBarImage != null)
        {
            chargeBar.value = Mathf.Floor(lerpTime * 5) / 5f;
            chargeBarImage.color = Color.Lerp(new Color32(10, 10, 0, 255), new Color32(255, 200, 0, 255), Mathf.Floor(lerpTime * 5) / 5f);
        }

        if (Time.time - effectTime >= _item.ChargeTime / 5f)
        {
            effectTime = Time.time;

            var effect = Instantiate(
                Resources.Load<Transform>("Effect/ChargeEffect"),
                transform.position + (Random.onUnitSphere * 0.15f),
                Quaternion.Euler(0, 0, Random.Range(0, 360)),
                transform
            );

            effect.localScale = Vector3.one * ((lerpTime * 0.6f) + 0.4f) * 0.3f;
        }

        float rotationZ = voidTransform.rotation.eulerAngles.z;

        if (rotationZ > 180f) rotationZ -= 360f;

        if (rotatingRight)
        {
            voidTransform.rotation *= Quaternion.Euler(0, 0, Time.deltaTime * lerpTime * 1200);
            if (rotationZ >= 20)
                rotatingRight = false;
        }
        else
        {
            voidTransform.rotation *= Quaternion.Euler(0, 0, -Time.deltaTime * lerpTime * 1200);
            if (rotationZ <= -20)
                rotatingRight = true;
        }

        speedOnCharge = Mathf.Lerp(_item.MinSpeed, _item.MaxSpeed, lerpTime);
        damageOnCharge = Mathf.Lerp(_item.MinDamage, _item.MaxDamage, lerpTime);
    }

    public void Attack() => StartCoroutine(AttackIE());

    private IEnumerator AttackIE()
    {
        var mousePoint = Input.mousePosition;

        var mousePos = Camera.main.ScreenToWorldPoint(mousePoint);

        var direction = (mousePos - transform.position).ConvertTo<Vector2>().normalized;

        var plrPos = _item.Owner.Character.transform.position + _item.Owner.Character.Offset;

        Destroy(chargeCanvas.gameObject);

        var voidScale = voidTransform.localScale;
        float voidSize = Mathf.Max(voidScale.x, voidScale.y);

        var ray = Physics2D.Raycast(plrPos, -direction, 1.5f + voidSize, LayerMask.GetMask("Wall"));

        if (ray.collider != null)
            moveOffset = -direction * Mathf.Max(ray.distance - voidSize, 0);
        else
            moveOffset = -direction * 1.5f;

        lockMoveVoid = true;

        yield return new WaitForSeconds(0.1f);

        dustEffect = Instantiate(Resources.Load<ParticleSystem>("Effect/DustEffect"), transform.position, transform.rotation, transform);

        var animator = transform.GetComponentInChildren<Animator>();
        if (animator != null)
            animator.SetBool("Attacking", true);

        //_rigidbody.AddTorque(Mathf.Sign(Random.value - 0.5f) * 3, ForceMode2D.Impulse);

        float attackScale = 0.5f;

        attackScale = Mathf.Lerp(1, attackScale, speedOnCharge / 2f);

        var oldLocalScale = transform.localScale;

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        transform.localScale = new Vector3(oldLocalScale.x, oldLocalScale.y * attackScale, oldLocalScale.z);

        var velocity = direction * _speedMultiply * speedOnCharge * 25;

        SetVelocity(velocity);

        onItemStart?.Invoke(velocity);

        attacked = true;

        yield return new WaitForSeconds(_item.LifeTime);

        DropItem();
    }

    public void DropItem() => StartCoroutine(DropItemIE());

    public IEnumerator DropItemIE()
    {
        _rigidbody.freezeRotation = true;
        SetVelocity(Vector2.zero);

        if (dustEffect != null)
        {
            var emission = dustEffect.emission;
            emission.rateOverTime = 0;
            Destroy(dustEffect.gameObject, 5);
        }

        var animator = transform.GetComponentInChildren<Animator>();
        if (animator != null)
            animator.SetTrigger("Drop");

        yield return new WaitForSeconds(1);

        gameObject.SetActive(false);

        var voidItemDropObj = new GameObject();
        voidItemDropObj.transform.position = transform.position;
        var voidItemDrop = voidItemDropObj.AddComponent<VoidItemDrop>();

        onItemDestroy?.Invoke(voidItemDrop);

        voidItemDrop.Setup(_item);
        
        Destroy(gameObject);
    }

    public void SetVelocity(Vector2 velocity)
    {
        nowVelocity = velocity;
        _rigidbody.velocity = nowVelocity;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!attacked) return;

        if (!collision.CompareTag("Player") && !collision.CompareTag("Void") && collision.TryGetComponent(out ICombatEntity entity))
        {
            if (!_entityAttacked.Contains(entity))
            {
                _entityAttacked.Add(entity);

                Vector2 direction = (collision.transform.position - transform.position).normalized;

                entity.TakeDamage(AttackDamage);
                entity.ApplyForce(direction, _item.ForcePower / 3f);

                CameraController.Instance.TriggerShake(0.05f, 0.05f, 0.15f);

                if (collision.CompareTag("Object"))
                {
                    onItemHitEnemy?.Invoke(entity);
                    DropItem();
                }
                else
                    onItemHitEnemy?.Invoke(entity);
            }
        }
        else if (collision.CompareTag("Wall"))
        {
            onItemHitEnemy?.Invoke(null);
            DropItem();
        }
    }
}
