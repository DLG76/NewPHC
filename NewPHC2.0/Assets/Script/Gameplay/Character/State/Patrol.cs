using Unity.VisualScripting;
using UnityEngine;

public class Patrol : State
{
    private PolygonCollider2D spawnCollider;
    private float maxPatrolTime;
    private Vector2 patrolPos;

    public Patrol(EnemyCombat _enemy) : base(_enemy)
    {

    }

    protected override void Enter()
    {
        spawnCollider = enemy.transform.parent.GetComponent<PolygonCollider2D>();
        maxPatrolTime = Random.Range(2f, 5f);
        patrolPos = RandomPatrolPosition();

        var enemyPos = (enemy.transform.position + enemy.Offset).ConvertTo<Vector2>();

        var ray = Physics2D.Raycast(enemyPos, patrolPos - enemyPos, Mathf.Infinity, LayerMask.GetMask("Wall"));

        if (ray.collider != null)
            patrolPos = ray.point;

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

        var enemyPos = (enemy.transform.position + enemy.Offset).ConvertTo<Vector2>();
        var direction = patrolPos - enemyPos;

        var closestTarget = GetClosestTarget();

        if (closestTarget != null)
        {
            Vector2 targetPos = closestTarget.transform.position + closestTarget.Offset;

            float distance = (targetPos - enemyPos).magnitude;

            if (distance <= enemy.ChaseDistance && CanSeeTarget(closestTarget))
            {
                nextState = new Chase(enemy);
                _event = EVENT.EXIT;
                return;
            }
        }
        
        if (runTime <= maxPatrolTime && direction.magnitude > 0.5f)
        {
            onMoveState?.Invoke(direction);
        }
        else
        {
            nextState = new Idle(enemy);
            _event = EVENT.EXIT;
        }
    }

    private Vector2 RandomPatrolPosition()
    {
        if (spawnCollider == null || spawnCollider.points.Length == 0) return Vector3.zero;

        Vector2 randomPoint;
        do
        {
            Vector2 min = spawnCollider.bounds.min;
            Vector2 max = spawnCollider.bounds.max;
            randomPoint = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
        } while (!IsPointInPolygon(randomPoint, spawnCollider.points, spawnCollider.transform));

        return randomPoint;
    }

    private bool IsPointInPolygon(Vector2 point, Vector2[] pPoints, Transform pTransform)
    {
        Vector2 localPoint = pTransform.InverseTransformPoint(point);
        int j = pPoints.Length - 1;
        bool inside = false;

        for (int i = 0; i < pPoints.Length; i++)
        {
            if (pPoints[i].y < localPoint.y && pPoints[j].y >= localPoint.y || pPoints[j].y < localPoint.y && pPoints[i].y >= localPoint.y)
            {
                if (pPoints[i].x + (localPoint.y - pPoints[i].y) / (pPoints[j].y - pPoints[i].y) * (pPoints[j].x - pPoints[i].x) < localPoint.x)
                {
                    inside = !inside;
                }
            }
            j = i;
        }

        return inside;
    }
}
