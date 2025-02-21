using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChangePowerSkill", menuName = "Inventory/SkillItem/New Change Power Skill")]
public class ChangePowerSkill : SkillItem
{
    [SerializeField] private float speedMultiply = 1;
    [SerializeField] private float damageMultiply = 1;

    public override void Setup(VoidObject voidObject)
    {
        base.Setup(voidObject);

        voidObject.SpeedMultiply *= speedMultiply;
        voidObject.DamageMultiply *= damageMultiply;
    }
}
