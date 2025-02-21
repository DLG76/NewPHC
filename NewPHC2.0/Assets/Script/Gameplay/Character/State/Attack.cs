using Unity.VisualScripting;
using UnityEngine;

public class Attack : State
{
    public Attack(EnemyCombat _enemy) : base(_enemy)
    {

    }

    protected override void Enter()
    {
        onMoveState?.Invoke(Vector2.zero);
        // animator.SetBool("isWalking", false);

        onAttackState?.Invoke();

        base.Enter();
    }

    protected override void Exit()
    {
        base.Exit();
    }

    protected override void Update()
    {
        base.Update();

        if (runTime > Mathf.Min(enemy.AttackCooldown, 1))
        {
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

                float distance = (targetPos - enemyPos).magnitude;

                if (distance <= enemy.ChaseDistance && CanSeeTarget(closestTarget))
                {
                    nextState = new Chase(enemy);
                    _event = EVENT.EXIT;
                }
                else
                {
                    nextState = new Patrol(enemy);
                    _event = EVENT.EXIT;
                }
            }
        }
        else onMoveState?.Invoke(Vector2.zero);
    }
}