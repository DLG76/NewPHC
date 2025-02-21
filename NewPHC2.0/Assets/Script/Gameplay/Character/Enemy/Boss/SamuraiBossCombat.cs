using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class SamuraiBossCombat : BossCombat
{
    private bool fade2 = false;
    [SerializeField] private float tickSpeed = 1;
    [SerializeField] private float knockbackMultiply = 0.25f;
    [SerializeField] private Vector2 attackCast = new Vector2(10, 8);
    [SerializeField] private float attackForce = 3;

    [System.Serializable]
    private class ThrowStoneTableData
    {
        public Vector3 startPos;
        public float distance;
        public Quaternion rotation;
    }

    protected override void Awake()
    {
        _chaseType = ChaseType.Player;

        _attackDistance = Mathf.Infinity;
        _chaseDistance = Mathf.Infinity;

        base.Awake();
    }

    protected override void Update()
    {
        base.Update();

        state = state.Process();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;

        var enemyPos = transform.position;

        foreach (var plr in ServerManager.Characters)
        {
            var plrPos = plr.transform.position;
            plrPos.z = enemyPos.z;
            var direction = (plrPos - enemyPos).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Gizmos.matrix = Matrix4x4.TRS(transform.position + (direction * (attackCast.x / 2f)), Quaternion.Euler(0, 0, angle), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, attackCast);
        }
    }

    public override void TakeDamage(float dmg)
    {
        if (_health - dmg < _maxHealth * 0.5f && !fade2)
        {
            Fade2();
            return;
        }

        base.TakeDamage(dmg);
    }

    private void Fade2() => StartCoroutine(Fade2IE());

    private IEnumerator Fade2IE()
    {
        if (fade2) yield break;

        fade2 = true;

        _maxHealth *= 0.75f;
        _health = _maxHealth;

        tickSpeed *= 1.3f;

        var startScale = transform.localScale;

        var globalLight = FindObjectsOfType<Light2D>().Where(l => l.lightType == Light2D.LightType.Global).FirstOrDefault();

        transform.localScale *= 1.5f;
        globalLight.intensity /= 2;
    }

    private void OnDestroy()
    {
        var globalLight = FindObjectsOfType<Light2D>().Where(l => l.lightType == Light2D.LightType.Global).FirstOrDefault();
        if (globalLight != null)
            globalLight.intensity *= 2;
    }

    private IEnumerator Slash()
    {
        var enemyPos = transform.position + _offset;
        var plr = PlayerCombat.ClosestPlayerInstance(enemyPos);

        if (plr != null)
        {
            var plrPos = plr.transform.position + plr.Offset;
            plrPos.z = enemyPos.z;
            var direction = (plrPos - enemyPos).normalized;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            var boxGuideLineMidel = Resources.Load<Transform>("GuideLine/RedBox");
            var boxGuideLine = Instantiate(boxGuideLineMidel, enemyPos + (direction * (attackCast.x / 2f)), Quaternion.Euler(0, 0, angle));
            boxGuideLine.localScale = attackCast;

            var boxGuideLineTime = Instantiate(boxGuideLineMidel, enemyPos, Quaternion.Euler(0, 0, angle));
            boxGuideLineTime.localScale = new Vector2(0, attackCast.y);
            boxGuideLineTime.DOMove(enemyPos + (direction * (attackCast.x / 2f)), 0.4f);
            boxGuideLineTime.DOScale(attackCast, 0.4f);

            yield return new WaitForSeconds(0.25f);

            _animator.SetFloat("AttackState", 0);
            _animator.SetFloat("NormalState", 0);
            _animator.SetFloat("SkillState", 0);
            _animator.SetTrigger("Attack");

            yield return new WaitForSeconds(0.15f);

            Destroy(boxGuideLine.gameObject);
            Destroy(boxGuideLineTime.gameObject);

            var rays = Physics2D.BoxCastAll(enemyPos + (direction * (attackCast.x / 2f)), attackCast, angle, Vector3.zero, Mathf.Infinity, LayerMask.GetMask("Player"));

            if (rays.Length > 0)
                CameraController.Instance?.TriggerShake(0.06f, 0.5f, 0.3f);

            foreach (var ray in rays)
            {
                if (ray.collider != null && ray.transform.TryGetComponent(out PlayerCombat player))
                {
                    player.TakeDamage(attackDamage);
                    player.ApplyForce(direction, attackForce);
                }
            }
        }
    }

    private IEnumerator ThrowSword1()
    {
        int swordCount = Random.Range(7, 15);
        float speed = 2 * tickSpeed;

        var plr = ServerManager.RandomCharacter;

        var oldPlrPos = plr.transform.position + plr.Offset;

        for (int i = 0; i < swordCount; i++)
        {
            if (plr == null || plr.gameObject == null)
            {
                plr = ServerManager.RandomCharacter;

                if (plr == null)
                    yield break;

                oldPlrPos = plr.transform.position + plr.Offset;
            }

            var plrPos = plr.transform.position + plr.Offset;
            var enemyPos = transform.position + _offset;
            plrPos.z = enemyPos.z;

            var direction = (plrPos - enemyPos).normalized;
            float distance = (plrPos - enemyPos).magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            var oldDirection = (oldPlrPos - enemyPos).normalized;
            float oldDistance = (oldPlrPos - enemyPos).magnitude;
            float oldAngle = Mathf.Atan2(oldDirection.y, oldDirection.x) * Mathf.Rad2Deg;

            float angleAdd = (angle - oldAngle) * ((oldDistance + distance) * (1 + (0.4f / tickSpeed)) / speed / 35f * distance * Mathf.Min(1, Random.value * 1.5f));

            var sword = Instantiate(Resources.Load<ProjectileObject>("Weapon/Sword"), enemyPos - (direction * 4), Quaternion.Euler(0, 0, angle + angleAdd - 90));

            sword.Setup(attackDamage * 0.75f, Quaternion.Euler(0, 0, angleAdd) * direction, 0.4f / tickSpeed, speed);

            oldPlrPos = plrPos;

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ThrowSword2()
    {
        int swordCount = Random.Range(2, 6);
        float speed = 2 * tickSpeed;

        var plr = ServerManager.RandomCharacter;

        for (int i = 0; i < swordCount; i++)
        {
            if (plr == null || plr.gameObject == null)
            {
                plr = ServerManager.RandomCharacter;

                if (plr == null)
                    yield break;
            }

            var plrPos = plr.transform.position + plr.Offset;
            var enemyPos = transform.position + _offset;
            plrPos.z = enemyPos.z;

            var direction = (plrPos - enemyPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            var randomAdded = Random.insideUnitSphere * 3;

            for (int j = -2; j < 3; j++)
            {
                var addRotation = Quaternion.Euler(0, 0, j * 5);

                var sword = Instantiate(Resources.Load<ProjectileObject>("Weapon/Sword"), enemyPos - (direction * 4) + randomAdded, Quaternion.Euler(0, 0, angle - 90) * addRotation);

                sword.Setup(attackDamage * 0.4f, addRotation * direction, 0.75f / tickSpeed, speed);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    protected override IEnumerator AttackIE()
    {
        if (!_canAttack || ServerManager.Characters.Length == 0) yield break;

        _canAttack = false;

        Move(Vector2.zero);

        switch (PlayerInArea(attackCast.x) ? Random.Range(0, 4) : Random.Range(2, 4))
        {
            case 0 | 1:
                yield return Slash();
                break;
            case 2:
                yield return ThrowSword1();
                break;
            case 3:
                yield return ThrowSword2();
                break;
        }

        yield return new WaitForSeconds(_attackCooldown / tickSpeed);

        _canAttack = true;
    }

    private bool PlayerInArea(float range)
    {
        var enemyPos = transform.position + _offset;

        foreach (var plr in ServerManager.Characters)
        {
            var plrPos = plr.transform.position + plr.Offset;
            plrPos.z = enemyPos.z;
            float distance = (plrPos - enemyPos).magnitude;

            if (distance <= range)
                return true;
        }

        return false;
    }

    protected override IEnumerator ApplyForceIE(Vector2 vector, float power)
    {
        return base.ApplyForceIE(vector, power * knockbackMultiply);
    }
}
