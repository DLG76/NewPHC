using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RangedEnemyCombat : EnemyCombat
{
    [SerializeField] private float ballSpeed = 1;

    protected override void Awake()
    {
        _chaseType = ChaseType.Player;

        base.Awake();
    }

    protected override IEnumerator AttackIE()
    {
        if (!_canAttack) yield break;

        _canAttack = false;

        AudioManager.Instance?.PlaySound("ThrowingBall");

        _animator.SetFloat("AttackState", 0);
        _animator.SetFloat("NormalState", 0);
        _animator.SetTrigger("Attack");

        var ball = CreateBall();
        ball.Setup(attackDamage, ballSpeed);

        yield return new WaitForSeconds(_attackCooldown);

        _canAttack = true;
    }

    private EnemyBall CreateBall()
    {
        var ball = Instantiate(Resources.Load<Transform>("Weapon/Ball"));
        ball.position = transform.position;
        return ball.AddComponent<EnemyBall>();
    }

    [RequireComponent(typeof(Rigidbody2D))]
    private class EnemyBall : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private bool isReady = false;
        private float damage;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.freezeRotation = true;
            _rigidbody.isKinematic = false;
            _rigidbody.gravityScale = 0;
            

            Destroy(gameObject, 10);
        }

        public void Setup(float damage, float ballSpeed)
        {
            isReady = true;

            this.damage = damage;

            if (ServerManager.Characters.Length > 0)
            {
                var myPos = transform.position;
                var plr = PlayerCombat.ClosestPlayerInstance(myPos);
                var plrPos = plr.transform.position + plr.Offset;
                plrPos.z = myPos.z;

                _rigidbody.velocity = (plrPos - myPos).normalized * ballSpeed * 10;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isReady) return;

            if (collision.TryGetComponent(out PlayerCombat player))
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
