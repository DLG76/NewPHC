using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class PlayerCombat : CharacterCombat
{
    public static PlayerCombat LocalPlayerInstance
    {
        get
        {
            if (ServerManager.Characters.Length > 0)
            {
                if (ServerManager.IsSinglePlayer || ServerManager.Characters.Length == 1)
                    return ServerManager.Characters[0];
                else
                    return ServerManager.Players.First(p => p.IsLocalPlayer).Character;
            }

            return null;
        }
    }

    public static PlayerCombat ClosestPlayerInstance(Vector3 pos)
    {
        if (ServerManager.Characters.Length > 0)
        {
            PlayerCombat closestPlayer = null;
            float distance = Mathf.Infinity;

            foreach (var cha in ServerManager.Characters)
            {
                if (cha._health > 0)
                {
                    var plrPos = cha.transform.position + cha.Offset;
                    pos.z = plrPos.z;

                    float newDistance = (plrPos - pos).magnitude;
                    if (newDistance < distance)
                    {
                        closestPlayer = cha;
                        distance = newDistance;
                    }
                }
            }

            return closestPlayer;
        }

        return null;
    }

    public PlayerCombatData playerData;

    [Header("Dash")]
    [SerializeField] private float dashForce = 1;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashDelay = 0.4f;
    private bool canDash = true;

    [Header("Dash Attack")]
    [SerializeField] private string[] attackItems;
    [SerializeField] private float dashAttackArea = 2.5f;
    [SerializeField] private float dashAttackDamage = 10;
    [SerializeField] private float dashAttackForce = 1;
    [SerializeField] private float dashAttackDelay = 5;
    private bool canDashAttack = true;

    [Header("Body")]
    [SerializeField] private SPUM_SpriteList body;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private float trailTime = 0.15f;

    [Header("Void")]
    [SerializeField] private VoidItem voidSelected;
    [SerializeField] private float collectRadious = 5;
    [SerializeField] private float impactRadious = 5;
    private bool canAttack = true;

    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    protected override void Awake()
    {
        if (ServerManager.IsSinglePlayer)
        {
            playerData = new PlayerCombatData(true, this);
            ServerManager.AddPlayer(playerData);
        }

        trail.widthMultiplier = 0;

        VoidHotbarUI.Instance.Setup(this);

        base.Awake();
    }

    private void Update()
    {
        var dungeon = DungeonManager.Instance;

        if (dungeon != null && dungeon.IsEndDungeon)
            _health = _maxHealth;

        if (Input.GetMouseButtonDown(0) &&
            ((dungeon != null && dungeon.IsStartingWave) || dungeon == null))
        {
            if (voidSelected != null)
                Attack();
        }

        if (Input.GetMouseButtonDown(1))
            DashAttack();

        if (Input.GetKeyDown(KeyCode.Q))
            Dash();

        Vector2 movement = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (AudioManager.Instance != null)
            if (movement.magnitude > 0 && !AudioManager.Instance.AudioSourceLoopPlayings.Contains("Footstep"))
                AudioManager.Instance?.PlaySound("Footstep", true, GetHashCode());
            else if (movement.magnitude == 0 && AudioManager.Instance.AudioSourceLoopPlayings.Contains("Footstep"))
                AudioManager.Instance?.StopSound("Footstep", GetHashCode());

        Move(movement);

        CheckItemInArea();
    }

    private void OnEnable()
    {
        if (LocalPlayerInstance == this)
        {
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }
    }

    private void CheckItemInArea()
    {
        foreach (var voidItemDrop in new List<VoidItemDrop>(VoidItemDrop.items))
        {
            if (voidItemDrop == null || voidItemDrop.gameObject == null && voidItemDrop.Item?.Owner?.Character != this)
                continue;

            if ((transform.position.ConvertTo<Vector2>() - voidItemDrop.transform.position.ConvertTo<Vector2>()).magnitude <= collectRadious)
                voidItemDrop.MoveTo(this);

            if ((transform.position - voidItemDrop.transform.position).magnitude <= Mathf.Min(collectRadious, 1))
            {
                //Impact(voidItemDrop.Item);
                voidItemDrop.Collect();
            }
        }
    }

    public bool SelectVoidItem(VoidItem voidItem)
    {
        if (canAttack)
        {
            voidSelected = voidItem;

            return true;
        }

        return false;
    }

    protected override IEnumerator AttackIE()
    {
        if (voidSelected == null || !canAttack)
            yield break;

        canAttack = false;

        voidSelected.Owner = playerData;

        var voidGameObject = new GameObject(voidSelected.Name + voidSelected.GetHashCode());
        voidGameObject.transform.position = transform.position;
        var voidObject = voidGameObject.AddComponent<VoidObject>();
        voidObject.Setup(voidSelected);

        virtualCamera.Follow = voidGameObject.transform;
        virtualCamera.LookAt = voidGameObject.transform;

        float endCameraSize = CameraController.Instance.CameraSize - 0.75f;

        while (Input.GetMouseButton(0))
        {
            voidObject.Charge();

            CameraController.Instance.LerpCameraSize(endCameraSize, voidSelected.ChargeTime);

            yield return null;
        }

        if (voidObject == null)
        {
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;

            canAttack = true;

            yield break;
        }

        VoidHotbarUI.Instance.RemoveItem(voidSelected);
        voidSelected = null;

        CameraController.Instance.StopLerpCameraSize();

        voidObject.Attack();

        AudioManager.Instance?.PlaySound("ThrowingVoid");

        var oldRotation = transform.rotation;
        var plrPos = (transform.position + _offset).ConvertTo<Vector2>();
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition).ConvertTo<Vector2>();

        var mouseDirection = (mousePos - plrPos).normalized;

        if (mouseDirection.x > 0)
            transform.rotation = Quaternion.Euler(oldRotation.x, 180, oldRotation.z);
        else if (mouseDirection.x < 0)
            transform.rotation = Quaternion.Euler(oldRotation.x, 0, oldRotation.z);

        _animator.SetFloat("AttackState", 0.35f);
        _animator.SetFloat("NormalState", 0);
        _animator.SetTrigger("Attack");

        virtualCamera.Follow = transform;
        virtualCamera.LookAt = transform;

        canRotate = false;

        yield return new WaitForSeconds(0.5f);

        canRotate = true;
        canAttack = true;
    }

    private void DashAttack() => StartCoroutine(DashAttackIE());

    private IEnumerator DashAttackIE()
    {
        if (!canDashAttack) yield break;

        canDashAttack = false;

        var enemyLayer = LayerMask.GetMask("Enemy");
        var plrPos = (transform.position + _offset).ConvertTo<Vector2>();
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition).ConvertTo<Vector2>();

        var mouseDirection = (mousePos - plrPos).normalized;

        var dashRay = Physics2D.Raycast(plrPos, mouseDirection, (mousePos - plrPos).magnitude, LayerMask.GetMask("Wall"));

        canWalk = false;

        trail.widthMultiplier = 1;
        trail.time = trailTime;

        var oldRotation = transform.rotation;

        string attackItem = null;

        if (attackItems.Length > 0)
            attackItem = attackItems[Random.Range(0, attackItems.Length)];

        if (attackItem != null)
        {
            var itemModel = Resources.Load<SpriteRenderer>($"Weapon/{attackItem}");

            body.SetSprite("L_Weapon", itemModel);
        }

        if (mouseDirection.x > 0)
            transform.rotation = Quaternion.Euler(oldRotation.x, 180, oldRotation.z);
        else if (mouseDirection.x < 0)
            transform.rotation = Quaternion.Euler(oldRotation.x, 0, oldRotation.z);

        _animator.SetFloat("AttackState", 1);
        _animator.SetFloat("NormalState", 0.333f);
        _animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.2f);

        _rigidbody.velocity = Vector2.zero;

        var dashPos = (dashRay.collider != null ? dashRay.point : mousePos) - _offset.ConvertTo<Vector2>();
        var direction = (dashPos - plrPos).normalized;
        var movePos = dashPos - (direction * 1.25f);

        transform.position = movePos;

        if (attackItem != null)
        {
            AudioManager.Instance?.PlaySound($"{attackItem}Break");

            var trashDestroyEffect = Instantiate(Resources.Load<Transform>($"Effect/{attackItem}DestroyEffect"), dashPos.ConvertTo<Vector3>() + _offset, Quaternion.identity);

            Destroy(trashDestroyEffect.gameObject, 7.5f);
        }

        var attackPos = dashPos + _offset.ConvertTo<Vector2>();

        var attackRays = Physics2D.CircleCastAll(attackPos, dashAttackArea, Vector2.zero, Mathf.Infinity, enemyLayer);

        foreach (var attackRay in attackRays)
        {
            var ray = Physics2D.Raycast(attackPos, (attackRay.point - attackPos).normalized, attackRay.distance, LayerMask.GetMask("Wall"));

            if (ray.collider != null)
                continue;

            if (attackRay.transform.TryGetComponent(out ICombatEntity entity))
            {
                var attackDirection = (attackRay.point - movePos).normalized;

                entity.TakeDamage(dashAttackDamage);
                entity.ApplyForce(attackDirection, dashAttackForce * 5);
            }
        }

        yield return new WaitForSeconds(0.2f);

        if (attackItem != null)
            body.SetSprite("L_Weapon", null);

        trail.DOTime(0, trailTime);
        trail.widthMultiplier = 0;

        canWalk = true;

        yield return new WaitForSeconds(dashAttackDelay);

        canDashAttack = true;
    }

    private void Dash() => StartCoroutine(DashIE());

    private IEnumerator DashIE()
    {
        if (!canDash) yield break;

        canDash = false;

        var plrPos = transform.position;
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = plrPos.z;
        var direction = _rigidbody.velocity.normalized;

        if (direction != Vector2.zero)
        {
            canWalk = false;

            AudioManager.Instance?.PlaySound("Dash", 1.75f);

            trail.widthMultiplier = 1;
            trail.time = trailTime;

            _rigidbody.AddRelativeForce(direction * dashForce * 40, ForceMode2D.Impulse);

            yield return new WaitForSeconds(dashDuration - 0.025f);

            _rigidbody.velocity = Vector2.zero;

            yield return new WaitForSeconds(0.025f);

            trail.DOTime(0, trailTime);
            trail.widthMultiplier = 0;

            canWalk = true;

            yield return new WaitForSeconds(dashDelay);
        }

        canDash = true;
    }

    public override void TakeDamage(float dmg)
    {
        var dungeon = DungeonManager.Instance;

        if (dungeon != null && dungeon.IsEndDungeon) return;

        CameraController.Instance?.TriggerShake(0.075f, 0.175f, (Mathf.Min(dmg, _health) / _maxHealth * 1.5f) + 0.25f);
        CameraController.Instance?.Hurt();

        Instantiate(Resources.Load<Transform>("Effect/PlayerHitEffect"), RandomPositionGenerator.GetRandomPositionInCollider(_collider), Quaternion.identity);

        AudioManager.Instance?.PlaySound("PlayerHurt" + Random.Range(1, 4), 0.25f, Random.Range(1.1f, 1.35f));

        base.TakeDamage(dmg);
    }

    private void OnDestroy()
    {
        if (ServerManager.IsSinglePlayer)
        {
            ServerManager.RemovePlayer(playerData);
        }
    }

    protected virtual void OnDisable()
    {
        _rigidbody.velocity = Vector2.zero;

        if (AudioManager.Instance != null && AudioManager.Instance.AudioSourceLoopPlayings.Contains("Footstep"))
            AudioManager.Instance?.StopSound("Footstep", GetHashCode());
    }

    public override void Die()
    {
        base.Die();
    }
}
