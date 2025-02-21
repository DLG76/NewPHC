using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Idle : State
{
    private float maxIdleTime;

    public Idle(EnemyCombat _enemy) : base(_enemy)
    {

    }

    protected override void Enter()
    {
        maxIdleTime = Random.Range(1.5f, 5f);
        // animator.SetBool("isWalking", false);

        base.Enter();
    }

    protected override void Exit()
    {
        base.Exit();
    }

    protected override void Update()
    {
        base.Update();

        var enemyPos = (enemy.transform.position + enemy.Offset).ConvertTo<Vector2>();
        var closestTarget = GetClosestTarget();

        if (closestTarget != null)
        {
            Vector2 targetPos = closestTarget.transform.position + closestTarget.Offset;

            float distance = (targetPos - enemyPos).magnitude;

            if (distance <= enemy.AttackDistance && enemy.CanAttack && CanSeeTarget(closestTarget))
            {
                nextState = new Attack(enemy);
                _event = EVENT.EXIT;
                return;
            }
            else if (distance <= enemy.ChaseDistance && CanSeeTarget(closestTarget))
            {
                nextState = new Chase(enemy);
                _event = EVENT.EXIT;
                return;
            }
        }

        if (runTime <= maxIdleTime)
        {
            onMoveState?.Invoke(Vector2.zero);
        }
        else
        {
            nextState = new Patrol(enemy);
            _event = EVENT.EXIT;
        }
    }
}