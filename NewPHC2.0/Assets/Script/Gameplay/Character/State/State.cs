using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class State
{
    public UnityAction onAttackState;
    public UnityAction<Vector2> onMoveState;

    protected enum STATE
    {
        IDLE,
        PATROL,
        CHASE,
        ATTACK
    }

    protected enum EVENT
    {
        ENTER,
        UPDATE,
        EXIT
    }

    protected EVENT _event = EVENT.ENTER;
    protected float runTime;

    protected EnemyCombat enemy;
    // protected Animator animator;

    protected State nextState;

    public State(EnemyCombat _enemy)
    {
        enemy = _enemy;
        // animator = _enemy.GetComponent<Animator>();
    }

    protected virtual void Enter()
    {
        _event = EVENT.UPDATE;
        //Debug.Log("ENTER STATE " + GetType().Name);
    }

    protected virtual void Update()
    {
        _event = EVENT.UPDATE;
    }

    protected virtual void Exit()
    {
        _event = EVENT.UPDATE;
        //Debug.Log("EXIT STATE " + GetType().Name);
    }

    public State Process()
    {
        switch (_event)
        {
            case EVENT.ENTER:
                Enter();
                break;
            case EVENT.UPDATE:
                Update();
                break;
            case EVENT.EXIT:
                Exit();

                nextState.onAttackState = onAttackState;
                nextState.onMoveState = onMoveState;

                return nextState;
        }

        runTime += Time.deltaTime;

        return this;
    }

    protected CharacterCombat GetClosestTarget()
    {
        var enemyPos = enemy.transform.position + enemy.Offset;

        switch (enemy.chaseType)
        {
            case EnemyCombat.ChaseType.Player:
                return PlayerCombat.ClosestPlayerInstance(enemyPos);
            case EnemyCombat.ChaseType.Enemy:
                return EnemyCombat.ClosestEnemyInstance(new EnemyCombat[] {enemy}, enemy.BanEnemyChases.ToArray(), enemyPos);
            default:
                return null;
        }
    }

    protected bool CanSeeTarget(CharacterCombat cha)
    {
        Vector2 enemyPos = (enemy.transform.position + enemy.Offset).ConvertTo<Vector2>();
        Vector2 chaPos = (cha.transform.position + cha.Offset).ConvertTo<Vector2>();

        int layerMask;

        switch (enemy.chaseType)
        {
            case EnemyCombat.ChaseType.Player:
                layerMask = LayerMask.GetMask("Player", "Wall");
                break;
            case EnemyCombat.ChaseType.Enemy:
                layerMask = LayerMask.GetMask("Enemy", "Wall");
                break;
            default:
                return false;
        }

        var hits = Physics2D.RaycastAll(enemyPos, (chaPos - enemyPos).normalized, Vector2.Distance(enemyPos, chaPos), layerMask).OrderBy(hit => hit.distance);

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                return false;
        }

        return true;
    }

}
