using Unity.VisualScripting;
using UnityEngine;

public class Chase : State
{
    public Chase(EnemyCombat _enemy) : base(_enemy)
    {

    }

    protected override void Enter()
    {
        // animator.SetBool("isWalking", true);

        base.Enter();
    }

    protected override void Exit()
    {
        base.Exit();
    }

    protected override void Update()
    {
        base.Update();

        if (ServerManager.Characters.Length == 0 && enemy.chaseType == EnemyCombat.ChaseType.Player)
        {
            nextState = new Patrol(enemy);
            _event = EVENT.EXIT;
            return;
        }

        var enemyPos = (enemy.transform.position + enemy.Offset).ConvertTo<Vector2>();
        var closestTarget = GetClosestTarget();

        if (closestTarget != null)
        {
            Vector2 targetPos = closestTarget.transform.position + closestTarget.Offset;

            var direction = (targetPos - enemyPos).normalized;
            float distance = (targetPos - enemyPos).magnitude;

            if (distance <= enemy.AttackDistance && enemy.CanAttack && CanSeeTarget(closestTarget))
            {
                nextState = new Attack(enemy);
                _event = EVENT.EXIT;
            }
            else if (distance <= enemy.ChaseDistance && CanSeeTarget(closestTarget))
            {
                onMoveState?.Invoke(direction);
            }
            else
            {
                nextState = new Idle(enemy);
                _event = EVENT.EXIT;
            }
        }
        else
        {
            nextState = new Idle(enemy);
            _event = EVENT.EXIT;
        }
    }
}