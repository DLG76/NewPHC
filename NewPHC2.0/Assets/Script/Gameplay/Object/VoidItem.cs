using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVoidItem", menuName = "Inventory/New Void Item")]
public class VoidItem : Item
{
    public string ID { get => id; }

    public PlayerCombatData Owner;
    public Transform voidPrefab;
    public int MaxSkillEquipment = 2;
    public List<SkillItem> SkillItems;
    public Vector2 Offset = new Vector2(0, 2);
    public float MinDamage = 3;
    public float MaxDamage = 10;
    public float MinSpeed = 0.3f;
    public float MaxSpeed = 1;
    public float ChargeTime = 1;
    public float ForcePower = 1;
    public float LifeTime = 5;

    [HideInInspector] public bool IsRealInstance = true;

    public enum EquipErrorType
    {
        MaxSlot,
        HaveSingle,
        None
    }

    public static VoidItem CreateNewItem(Item tempItem)
    {
        VoidItem item = tempItem as VoidItem;
        var newItem = CreateInstance<VoidItem>();

        var fields = item.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.IsPublic || field.IsDefined(typeof(SerializeField), true))
            {
                var value = field.GetValue(item);
                field.SetValue(newItem, value);
            }
        }

        newItem.SkillItems = item.SkillItems.Select(i => SkillItem.CreateNewItem(i)).ToList();

        newItem.IsRealInstance = false;

        return newItem;
    }

    public EquipErrorType AddSkill(SkillItem skillItem)
    {
        if (!skillItem.IsRealInstance)
        {
            var oldSkill = SkillItems.Find(s => s.skillType == skillItem.skillType);
            if (oldSkill != null)
                if (oldSkill.IsDefault && skillItem.IsSingle)
                    SkillItems.Remove(oldSkill);
                else if (oldSkill.IsSingle && skillItem.IsSingle)
                    return EquipErrorType.HaveSingle;
        }

        if (SkillItems.Count < MaxSkillEquipment)
        {
            if (skillItem.IsRealInstance)
            {
                skillItem = SkillItem.CreateNewItem(skillItem);
            }

            SkillItems.Add(skillItem);

            return EquipErrorType.None;
        }

        return EquipErrorType.MaxSlot;
    }
}
