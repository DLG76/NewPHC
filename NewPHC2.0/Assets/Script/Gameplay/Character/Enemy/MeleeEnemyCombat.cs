using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyCombat : EnemyCombat
{
    private List<PlayerCombat> playerHits = new List<PlayerCombat>();

    protected override void Awake()
    {
        _chaseType = ChaseType.Player;

        _attackDistance = 0;

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

        yield return new WaitForSeconds(_attackCooldown);

        _canAttack = true;
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
