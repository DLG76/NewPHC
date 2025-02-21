using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeEnemyCombat : EnemyCombat
{
    private bool canMoveStep = true;
    [SerializeField] private float moveStepTime = 0.1f;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private float trailTime = 0.15f;

    private List<PlayerCombat> playerHits = new List<PlayerCombat>();

    protected override void Awake()
    {
        _chaseType = ChaseType.Player;

        _attackDistance = 0;

        trail.enabled = false;
        trail.time = trailTime;

        base.Awake();
    }

    protected override IEnumerator AttackIE()
    {
        if (!_canAttack) yield break;

        _canAttack = false;

        var enemyPos = transform.position;
        var plr = PlayerCombat.ClosestPlayerInstance(enemyPos);

        if (plr != null)
        {
            AudioManager.Instance?.PlaySound("EnemyMeleeAttack", 0.25f, Random.Range(0.8f, 1.2f));

            _animator.SetFloat("AttackState", 0);
            _animator.SetFloat("NormalState", 1);
            _animator.SetTrigger("Attack");

            var plrPos = plr.transform.position;
            plrPos.z = enemyPos.z;

            var direction = (plrPos - enemyPos).normalized;

            if (playerHits.Count > 0)
            {
                ApplyForce(-direction, 2.5f);

                foreach (var player in playerHits)
                {
                    var plrCastPos = player.transform.position;
                    plrCastPos.z = enemyPos.z;

                    var directionCast = (plrCastPos - enemyPos).normalized;

                    player.TakeDamage(attackDamage);
                    player.ApplyForce(directionCast, 5);
                }
            }
        }

        yield break;
    }

    protected override void Move(Vector2 vector) => StartCoroutine(MoveIE(vector));

    private IEnumerator MoveIE(Vector2 vector)
    {
        if (!canMoveStep) yield break;

        canMoveStep = false;

        _canAttack = true;

        AudioManager.Instance?.PlaySound("EnemyCharge", 0.1f, Random.Range(0.8f, 1.2f));

        trail.enabled = true;
        base.Move(vector);

        yield return new WaitForSeconds(moveStepTime);

        base.Move(Vector2.zero);

        yield return new WaitForSeconds(trailTime);

        trail.enabled = false;

        yield return new WaitForSeconds(Mathf.Max(_attackCooldown, 0));

        _canAttack = false;

        canMoveStep = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerCombat player))
        {
            if (!playerHits.Contains(player))
                playerHits.Add(player);

            Attack();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerCombat player))
        {
            if (playerHits.Contains(player))
                playerHits.Remove(player);
        }
    }
}
