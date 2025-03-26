using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class VoidItem : Item
{
    public PlayerCombatData Owner;
    public Transform voidPrefab;
    public List<SkillItem> SkillItems = new List<SkillItem>();
    public Vector2 Offset = new Vector2(0, 2);
    public float MinDamage { get; set; }
    public float MaxDamage { get; set; }
    public float MinSpeed { get; set; }
    public float MaxSpeed { get; set; }
    public float ChargeTime { get; set; }
    public float ForcePower { get; set; }
    public float LifeTime { get; set; }

    public VoidItem(JObject itemJson) : base(itemJson)
    {
        voidPrefab = Resources.Load<Transform>("Void/" + itemJson["name"].ToString());

        MinDamage = itemJson["minDamage"]?.Value<float>() ?? 3;
        MaxDamage = itemJson["maxDamage"]?.Value<float>() ?? 10;
        MinSpeed = itemJson["minSpeed"]?.Value<float>() ?? 0.3f;
        MaxSpeed = itemJson["maxSpeed"]?.Value<float>() ?? 1;
        ChargeTime = itemJson["chargeTime"]?.Value<float>() ?? 1;
        ForcePower = itemJson["forcePower"]?.Value<float>() ?? 1;
        LifeTime = itemJson["lifeTime"]?.Value<float>() ?? 5;

        var skillItems = SkillItem.GetAllSkillItems();

        foreach (var skillData in itemJson["skills"])
        {
            var skillName = skillData["skillName"].Value<string>();
            var level = skillData["level"]?.Value<int>() ?? 1;

            if (skillName != null && skillItems.FirstOrDefault(si => si.Name == skillName && si.Level == level) != null)
            {
                var skill = skillItems.FirstOrDefault(si => si.Name == skillName && si.Level == level);
                AddSkill(skill);
            }
        }

        if (SkillItems.Count == 0)
            AddSkill(skillItems.FirstOrDefault(si => si is BreakableSkill && si.Level == 1));
    }

    private void AddSkill(SkillItem skillItem)
    {
        if (skillItem == null)
            return;

        if (!skillItem.IsRealInstance)
        {
            var oldSkill = SkillItems.Find(s => s.skillType == skillItem.skillType);
            if (oldSkill != null)
                if (oldSkill.IsDefault && skillItem.IsSingle)
                    SkillItems.Remove(oldSkill);
                else if (oldSkill.IsSingle && skillItem.IsSingle)
                    return;
        }

        if (skillItem.IsRealInstance)
        {
            skillItem = SkillItem.CreateNewItem(skillItem);
        }

        SkillItems.Add(skillItem);
    }
}
