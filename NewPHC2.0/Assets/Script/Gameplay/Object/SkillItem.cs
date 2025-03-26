using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class SkillItem : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public string Description;

    public bool CanBuy;
    public int PriceBuy;
    public int PriceSell;

    public int Level = 1;
    public SkillType skillType;
    public bool IsSingle;
    public bool IsDefault;
    protected VoidObject voidObject;
    protected Transform guideLine;

    [HideInInspector] public bool IsRealInstance = true;

    public enum SkillType
    {
        Object,
        OnHit,
        Passive
    }

    public static List<SkillItem> GetAllSkillItems()
    {
        return Resources.LoadAll<SkillItem>("Skill").ToList();
    }

    public static T CreateNewItem<T>(T item) where T : SkillItem
    {
        var newItem = (T)CreateInstance(item.GetType());

        var fields = item.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if ((field.IsPublic || field.IsDefined(typeof(SerializeField), true)) && !field.IsStatic)
            {
                var value = field.GetValue(item);
                field.SetValue(newItem, value);
            }
        }

        newItem.IsRealInstance = false;

        return newItem;
    }

    public virtual void Setup(VoidObject voidObject)
    {
        this.voidObject = voidObject;
    }
}
