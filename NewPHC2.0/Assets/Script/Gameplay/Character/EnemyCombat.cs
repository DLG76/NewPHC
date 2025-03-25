using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VFX;

public abstract class EnemyCombat : CharacterCombat
{
    private static List<EnemyCombat> enemies = new List<EnemyCombat>();

    public static EnemyCombat ClosestEnemyInstance(EnemyCombat[] banEnemies, System.Type[] banEnemyTypes, Vector3 pos)
    {
        if (enemies.Count > 0)
        {
            EnemyCombat closestEnemy = null;
            float distance = Mathf.Infinity;

            foreach (var ene in enemies)
            {
                if (ene._health > 0 && banEnemies.FirstOrDefault(e => e == ene) == null && banEnemyTypes.FirstOrDefault(et => ene.GetType().Equals(et)) == null)
                {
                    var enemyPos = ene.transform.position + ene.Offset;
                    pos.z = enemyPos.z;

                    float newDistance = (enemyPos - pos).magnitude;
                    if (newDistance < distance)
                    {
                        closestEnemy = ene;
                        distance = newDistance;
                    }
                }
            }

            return closestEnemy;
        }

        return null;
    }

    [SerializeField] private string ID;

    [SerializeField] protected GameObject enemyCanvas;
    [SerializeField] protected Slider healthBar;

    public float AttackDistance { get => _attackDistance; }
    [SerializeField] protected float _attackDistance = 2.5f;
    public float ChaseDistance { get => _chaseDistance; }
    [SerializeField] protected float _chaseDistance = 7.5f;
    public bool CanAttack { get => _canAttack; }
    [SerializeField] protected bool _canAttack = true;
    public float AttackCooldown { get => _attackCooldown; }
    [SerializeField] protected float _attackCooldown = 2.5f;

    [SerializeField] protected float attackDamage = 10;

    [Header("Die Data")]
    [SerializeField] private EnemyEffectData hitEffectData;
    [SerializeField] private EnemyEffectData dieEffectData;

    public enum ChaseType
    {
        Enemy,
        Player
    }

    public ChaseType chaseType { get => _chaseType; }
    protected ChaseType _chaseType;
    public List<System.Type> BanEnemyChases { get => _banEnemyChases; }
    protected List<System.Type> _banEnemyChases = new List<System.Type>();

    protected State state;

    protected override void Awake()
    {
        base.Awake();

        state = new Idle(this);

        state.onAttackState += Attack;
        state.onMoveState += Move;

        if (enemyCanvas == null)
            enemyCanvas = GetComponentInChildren<Canvas>().gameObject;

        if (healthBar == null)
            healthBar = enemyCanvas.GetComponentInChildren<Slider>();

        healthBar.maxValue = _maxHealth;
        healthBar.value = _health;

        enemies.Add(this);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + _offset, _chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + _offset, _attackDistance);

        Gizmos.color = Color.magenta;
        foreach (var cha in ServerManager.Characters)
            Gizmos.DrawLine(transform.position + _offset, cha.transform.position + cha.Offset);
    }

    protected virtual void Update()
    {
        if (died)
        {
            enemyCanvas.SetActive(false);
            return;
        }

        enemyCanvas.SetActive(_health < _maxHealth);
        healthBar.maxValue = _maxHealth;
        healthBar.value = Mathf.Lerp(healthBar.value, _health, Time.deltaTime * 7.5f);

        healthBar.direction = transform.rotation.y == 0 ? Slider.Direction.LeftToRight : Slider.Direction.RightToLeft;

        state = state.Process();
    }

    public override void TakeDamage(float dmg)
    {
        if (died) return;

        ComboText.Instance?.AddCombo(dmg);
        
        var damageCanvas = Instantiate(Resources.Load<DamageTextUI>("UI/DamageCanvas"));

        damageCanvas.transform.position = transform.position;

        damageCanvas.Setup(dmg);

        AudioManager.Instance?.PlaySound("EnemyHurt", 0.25f, Random.Range(0.85f, 1.15f));

        base.TakeDamage(dmg);

        if (!died)
            hitEffectData.CreateEffect(_collider);
    }

    public override void Die()
    {
        died = true;

        CameraController.Instance.TriggerShake(0.07f, 0.15f, 0.3f);

        dieEffectData.CreateEffect(_collider);

        StartCoroutine(DieIE());
    }

    private IEnumerator DieIE()
    {
        _rigidbody.velocity = Vector2.zero;

        if (_animator != null)
        {
            _animator.SetFloat("RunState", 1);
            _animator.SetBool("Run", true);
        }

        yield return new WaitForSeconds(0.4f);

        base.Die();
    }

    private void OnDestroy()
    {
        enemies.Remove(this);
    }
}

[System.Serializable]
public class EnemyEffectData
{
    [SerializeField] private float effect1Siize = 1.5f;
    [SerializeField] private int effect2Count = 15;

    public void CreateEffect(Collider2D _collider)
    {
        var hitEffect = Object.Instantiate(Resources.Load<Transform>("Effect/EnemyHitEffect1"), RandomPositionGenerator.GetRandomPositionInCollider(_collider), Quaternion.identity);

        hitEffect.localScale *= effect1Siize;

        var dieEffect = Object.Instantiate(Resources.Load<Transform>("Effect/EnemyHitEffect2"), RandomPositionGenerator.GetRandomPositionInCollider(_collider), Quaternion.identity);

        dieEffect.GetComponent<VisualEffect>().SetInt("Count", effect2Count);

        Object.Destroy(dieEffect.gameObject, 3);
    }
}