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
    public List<SkillItem> SkillItems;
    public Vector2 Offset = new Vector2(0, 2);
    public float MinDamage = 3;
    public float MaxDamage = 10;
    public float MinSpeed = 0.3f;
    public float MaxSpeed = 1;
    public float ChargeTime = 1;
    public float ForcePower = 1;
    public float LifeTime = 5;

    public VoidItem(JObject itemJson, int count) : base(itemJson, count)
    {


        //SkillItems = new List<SkillItem>();
        //var skills = itemJson["skills"];
        //foreach (var skill in skills)
        //{
        //    var skillItem = ScriptableObject.CreateInstance<SkillItem>();
        //    skillItem.Name = skill["name"].ToString();
        //    skillItem.Icon = Resources.Load<Sprite>("SkillIcons/" + skill["icon"].ToString());
        //    skillItem.Description = skill["description"].ToString();
        //    skillItem.CanBuy = skill["canBuy"].ToObject<bool>();
        //    skillItem.PriceBuy = skill["priceBuy"].ToObject<int>();
        //    skillItem.PriceSell = skill["priceSell"].ToObject<int>();
        //    skillItem.Level = skill["level"].ToObject<int>();
        //    skillItem.skillType = (SkillItem.SkillType)skill["skillType"].ToObject<int>();
        //    skillItem.IsSingle = skill["isSingle"].ToObject<bool>();
        //    skillItem.IsDefault = skill["isDefault"].ToObject<bool>();
        //    SkillItems.Add(skillItem);
        //}
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
