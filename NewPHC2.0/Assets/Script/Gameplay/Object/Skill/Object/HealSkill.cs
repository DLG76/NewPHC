using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewHealSkill", menuName = "Inventory/SkillItem/New Heal Skill")]
public class HealSkill : SkillItem
{
    [SerializeField] private GameObject healItemModel;
    [SerializeField] private GameObject healEffectModel;
    [SerializeField] private float healPoint = 2;
    [SerializeField] private float healMaxCharge = 4;
    [SerializeField] private int healCount = 5;
    [SerializeField] private float healDelay = 1;

    private bool attackedEnemy = false;
    private float healMultiply = 0;

    public override void Setup(VoidObject voidObject)
    {
        base.Setup(voidObject);

        attackedEnemy = false;
        healMultiply = 0;

        voidObject.onItemHitEnemy += (entity) =>
        {
            if (entity != null)
            {
                attackedEnemy = true;
                healMultiply = Mathf.Min(healMultiply + (1 / healMaxCharge), 1);
            }
        };

        voidObject.onItemDestroy += (voidItemDrop) =>
        {
            if (attackedEnemy)
            {
                Instantiate(healItemModel, voidItemDrop.transform, false);

                voidItemDrop.onItemCollected += Heal;
            }
        };
    }

    private void Heal(PlayerCombat player)
    {
        var healingEntity = player.AddStatusEffect<HealingStatusEffect>();
        if (healingEntity != null)
            healingEntity.Setup(healDelay, healCount, healPoint * healMultiply);
    }
}
