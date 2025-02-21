using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBreakableSkill", menuName = "Inventory/SkillItem/New Breakable Skill")]
public class BreakableSkill : SkillItem
{
    private bool hitted = false;

    public override void Setup(VoidObject voidObject)
    {
        base.Setup(voidObject);

        hitted = false;

        voidObject.onItemHitEnemy += DestroySelf;
    }

    private void DestroySelf(ICombatEntity entity) => voidObject.StartCoroutine(DestroySelfIE(entity));

    private IEnumerator DestroySelfIE(ICombatEntity entity)
    {
        if (hitted) yield break;

        hitted = true;

        Destroy(voidObject.gameObject, 2);
        yield return voidObject.DropItemIE();
    }
}
