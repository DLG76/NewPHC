using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBombSkill", menuName = "InventoryUI/SkillItem/New Bomb Skill")]
public class BombSkill : SkillItem
{
    [SerializeField] private float bombSize = 1;
    [SerializeField] private float bombDamageMultiply = 1;
    [SerializeField] private float bombForcePower = 1;

    private bool hitted = false;

    public override void Setup(VoidObject voidObject)
    {
        base.Setup(voidObject);

        hitted = false;

        voidObject.onItemHitEnemy += Explode;
    }

    private void Explode(ICombatEntity entity)
    {
        if (hitted) return;

        hitted = true;

        var explosionEffect = Instantiate(Resources.Load<Transform>("Effect/Explosion"));
        explosionEffect.localScale = Vector3.one * bombSize * 1.5f;
        explosionEffect.position = voidObject.transform.position;

        var explosionDamage = explosionEffect.AddComponent<ExplosionDamage>();
        explosionDamage.Setup(voidObject.AttackDamage * bombDamageMultiply, bombForcePower);

        voidObject.DropItem();
    }

    private class ExplosionDamage : MonoBehaviour
    {
        private bool canTakeDamage = false;
        private List<ICombatEntity> entityTakedDamages = new List<ICombatEntity>();
        private float damage;
        private float forcePower;

        public void Setup(float damage, float forcePower)
        {
            this.damage = damage;
            this.forcePower = forcePower;
            entityTakedDamages.Clear();

            StartCoroutine(ExplosionIE());
        }

        private IEnumerator ExplosionIE()
        {
            canTakeDamage = true;

            CameraController.Instance.TriggerShake(0.04f, 0.5f, 0.25f);

            yield return new WaitForSeconds(0.5f);

            canTakeDamage = false;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!canTakeDamage) return;

            if (!collision.CompareTag("Player") && collision.TryGetComponent(out ICombatEntity entity))
            {
                var enemyPos = collision.transform.position;
                var voidPos = transform.position;
                var ray = Physics2D.Raycast(voidPos, (enemyPos - voidPos).normalized, (enemyPos - voidPos).magnitude, LayerMask.GetMask("Enemy", "Wall"));

                if (!entityTakedDamages.Contains(entity) && ray.collider != null && ray.transform.gameObject.Equals(collision.gameObject))
                {
                    entityTakedDamages.Add(entity);

                    var direction = (collision.transform.position - transform.position).normalized;

                    entity.TakeDamage(damage);
                    entity.ApplyForce(direction, forcePower);
                }
            }
        }
    }
}