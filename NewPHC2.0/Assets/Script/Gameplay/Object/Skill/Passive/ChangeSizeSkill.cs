using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChangeSizeSkill", menuName = "InventoryUI/SkillItem/New Change Size Skill")]
public class ChangeSizeSkill : SkillItem
{
    [SerializeField] private float sizeMultiply = 2;

    public override void Setup(VoidObject voidObject)
    {
        base.Setup(voidObject);

        voidObject.transform.localScale *= sizeMultiply;
    }
}
