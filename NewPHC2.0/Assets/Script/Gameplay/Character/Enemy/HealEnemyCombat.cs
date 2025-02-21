using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealEnemyCombat : EnemyCombat
{
    [SerializeField] private int attackTickCount = 6;
    [SerializeField] private float attackTickTime = 0.5f;

    protected override void Awake()
    {
        _chaseType = ChaseType.Enemy;

        _banEnemyChases.Add(typeof(HealEnemyCombat));

        base.Awake();
    }

    protected override IEnumerator AttackIE()
    {
        if (!_canAttack) yield break;

        _canAttack = false;

        _animator.SetFloat("AttackState", 0);
        _animator.SetFloat("NormalState", 0);
        _animator.SetTrigger("Attack");

        var enemyPos = transform.position;
        var eneTarget = ClosestEnemyInstance(new EnemyCombat[] {this}, _banEnemyChases.ToArray(), enemyPos);

        if (eneTarget != null)
        {
            var healingStatus = eneTarget.AddStatusEffect<HealingStatusEffect>();

            if (healingStatus != null)
                healingStatus.Setup(attackTickTime, attackTickCount, attackDamage);
        }

        yield return new WaitForSeconds(_attackCooldown);

        _canAttack = true;
    }
}
