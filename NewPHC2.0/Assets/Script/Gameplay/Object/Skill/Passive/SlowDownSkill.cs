using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "NewSlowDownSkill", menuName = "InventoryUI/SkillItem/New Slow Down Skill")]
public class SlowDownSkill : SkillItem
{
    [SerializeField] private float slowTime = 1.5f;
    [SerializeField] private int slowTick = 5;
    [SerializeField] private float slowMultiply = 0.7f;

    public override void Setup(VoidObject voidObject)
    {
        base.Setup(voidObject);

        voidObject.onItemHitEnemy += SlowEnemy;
    }

    private void SlowEnemy(ICombatEntity entity)
    {
        if (entity != null)
        {
            var slowDownHitEffect = Instantiate(Resources.Load<GameObject>("Effect/SlowDownHitEffect"), entity.GetTransform().position, Quaternion.identity);

            Destroy(slowDownHitEffect, 5);

            var slowingEffect = entity.AddStatusEffect<SlowingStatusEffect>();
            if (slowingEffect != null)
                slowingEffect.Setup(slowTime, slowTick, slowMultiply);
        }
    }
}