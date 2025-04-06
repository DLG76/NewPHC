using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MrXBossCombat : BossCombat
{
    private bool fade2 = false;
    private float throwStoneTime = 0;
    private float createWallTime = 0;
    private Transform wall;
    [SerializeField] private float tickSpeed = 1;
    [SerializeField] private ThrowStoneTableData[] throwStoneTableDatas;

    [SerializeField] private float roarDistance = 30;

    [System.Serializable]
    private class ThrowStoneTableData
    {
        public Vector3 startPos;
        public float distance;
        public Quaternion rotation;
    }

    protected override void Awake()
    {
        base.Awake();

        _speed = 0;
        _chaseDistance = 0;
        _attackDistance = 0;
        createWallTime = Time.time;
    }

    protected override void Update()
    {
        base.Update();

        if (_canAttack)
            Attack();

        RandomCreateWall();

        if (fade2)
            ThrowStone();
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (var throwStoneTableData in throwStoneTableDatas)
        {
            var from = throwStoneTableData.startPos + (throwStoneTableData.rotation * Vector2.right * throwStoneTableData.distance / 2f);
            var to = throwStoneTableData.startPos - (throwStoneTableData.rotation * Vector2.right * throwStoneTableData.distance / 2f);
            Gizmos.DrawLine(from, to);
        }

        Gizmos.color = Color.magenta;

        Gizmos.DrawWireCube(transform.position, Vector3.one * roarDistance);
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
        throwStoneTime = Time.time;

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

    private void ThrowStone()
    {
        if (Time.time - throwStoneTime < 1 || wall != null || ServerManager.Characters.Length == 0) return;

        throwStoneTime = Time.time;

        var randomCircle = (Random.insideUnitCircle.normalized * 12.5f).ConvertTo<Vector3>();
        var plr = ServerManager.RandomCharacter;
        var plrPos = plr.transform.position + plr.Offset;

        var stone = CreateStone(plrPos + randomCircle);

        stone.Setup(attackDamage, -randomCircle, 0.25f, 1.2f);
    }

    private void RandomCreateWall()
    {
        if (Time.time - createWallTime < 15 || wall != null || ServerManager.Characters.Length == 0) return;

        StartCoroutine(RandomCreateWallIE());
    }

    private IEnumerator RandomCreateWallIE()
    {
        createWallTime = Time.time;

        var plr = ServerManager.RandomCharacter;
        var plrPos = plr.transform.position + plr.Offset;

        var exclamationMask = Instantiate(Resources.Load<Transform>("GuideLine/ExclamationMask"));
        exclamationMask.position = plrPos;

        yield return new WaitForSeconds(1.5f);

        Destroy(exclamationMask.gameObject);

        wall = Instantiate(Resources.Load<Transform>("Weapon/StoneWallTrap"));
        wall.position = plrPos;
    }

    private ProjectileObject CreateStone(Vector3 position)
    {
        return Instantiate(Resources.Load<ProjectileObject>("Weapon/Stone"), position, Quaternion.identity);
    }

    private IEnumerator Roar()
    {
        var circleSize = Vector3.one * roarDistance;
        var bossPos = transform.position;

        var circleGuideLineMidel = Resources.Load<Transform>("GuideLine/RedCircle");
        var circleGuideLine = Instantiate(circleGuideLineMidel, bossPos, Quaternion.identity);
        circleGuideLine.localScale = circleSize;

        var circleGuideLineTime = Instantiate(circleGuideLineMidel, bossPos, Quaternion.identity);
        circleGuideLineTime.localScale = Vector3.zero;
        circleGuideLineTime.DOScale(circleSize, 2 / tickSpeed);

        yield return new WaitForSeconds(2 / tickSpeed);

        Destroy(circleGuideLine.gameObject);
        Destroy(circleGuideLineTime.gameObject);

        CameraController.Instance.TriggerShake(0.05f, 0.2f, 0.5f);

        var rays = Physics2D.CircleCastAll(bossPos, roarDistance / 2f, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Player"));

        foreach (var ray in rays)
        {
            if (ray.transform.TryGetComponent(out PlayerCombat player))
            {
                var plrPos = ray.point.ConvertTo<Vector3>();
                var direction = (plrPos - bossPos).normalized;

                player.TakeDamage(attackDamage * 2.5f);
                player.ApplyForce(direction, 7);
            }
        }
    }

    private IEnumerator RandomMultiplyThrowStone()
    {
        for (int i = 0; i < 5; i++)
        {
            var randomCircle = (Random.insideUnitCircle.normalized * 12.5f).ConvertTo<Vector3>();
            var plr = ServerManager.RandomCharacter;
            var plrPos = plr.transform.position + plr.Offset;

            var stone = CreateStone(plrPos + randomCircle);

            stone.Setup(attackDamage, -randomCircle, 0.75f / tickSpeed, 2.5f);

            yield return new WaitForSeconds(0.15f);
        }
    }

    private IEnumerator ThrowStoneTable()
    {
        void ThrowStoneWithTableData(ThrowStoneTableData data)
        {
            for (int i = 0; i <= data.distance; i += 5)
            {
                var stone = CreateStone(data.startPos + (data.rotation * Vector2.right * ((data.distance / 2f) - i)));

                stone.transform.localScale *= 2;

                stone.Setup(attackDamage, data.rotation * Vector2.down, 0.3f, 2);
            }
        }

        for (int i = 0; i < throwStoneTableDatas.Length; i++)
        {
            int randomIndex = Random.Range(i, throwStoneTableDatas.Length);
            ThrowStoneTableData temp = throwStoneTableDatas[i];
            throwStoneTableDatas[i] = throwStoneTableDatas[randomIndex];
            throwStoneTableDatas[randomIndex] = temp;
        }

        foreach (var throwStoneTableData in throwStoneTableDatas)
        {
            ThrowStoneWithTableData(throwStoneTableData);
            yield return new WaitForSeconds(1.5f / tickSpeed);
        }
    }

    private IEnumerator ThrowStoneShotgun()
    {
        var plr = ServerManager.RandomCharacter;
        var plrPos = plr.transform.position + plr.Offset;
        var randomCircle = (Random.insideUnitCircle.normalized * 15).ConvertTo<Vector3>();

        for (int i = 0; i < 10; i++)
        {
            var stone = CreateStone(plrPos + randomCircle);

            stone.Setup(attackDamage, Quaternion.Euler(0, 0, Random.Range(-30, 30)) * -randomCircle, 0.5f, tickSpeed);

            yield return new WaitForSeconds(Random.Range(0, 0.1f));
        }
    }

    protected override IEnumerator AttackIE()
    {
        if (!_canAttack || ServerManager.Characters.Length == 0) yield break;

        _canAttack = false;

        switch (Random.Range(0, 4))
        {
            case 0:
                yield return Roar();
                break;
            case 1:
                yield return RandomMultiplyThrowStone();
                break;
            case 2:
                yield return ThrowStoneTable();
                break;
            case 3:
                yield return ThrowStoneShotgun();
                break;
        }

        yield return new WaitForSeconds(_attackCooldown / tickSpeed);

        _canAttack = true;
    }
}
