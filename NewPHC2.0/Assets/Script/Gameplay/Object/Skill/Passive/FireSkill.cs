using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFireSkill", menuName = "InventoryUI/SkillItem/New Fire Skill")]
public class FireSkill : SkillItem
{
    [SerializeField] private float tickTime = 0.5f;
    [SerializeField] private int maxTickCount = 10;
    [SerializeField] private float burningDamageMultiply = 0.4f;
    [SerializeField] private Transform effectVoidModel;

    private List<ICombatEntity> entities = new List<ICombatEntity>();

    public override void Setup(VoidObject voidObject)
    {
        base.Setup(voidObject);

        if (effectVoidModel != null)
        {
            var effect = Instantiate(effectVoidModel, voidObject.transform, false);
        }

        entities.Clear();

        voidObject.onItemHitEnemy += BurnEnemy;
    }

    private void BurnEnemy(ICombatEntity entity)
    {
        if (entity != null && !entities.Contains(entity))
        {
            entities.Add(entity);

            CameraController.Instance.TriggerShake(0.045f, 0.14f, 0.15f);

            var burning = entity.AddStatusEffect<BurningStatusEffect>();
            if (burning != null)
                burning.Setup(tickTime, maxTickCount, voidObject.AttackDamage * burningDamageMultiply);
        }
    }
}
