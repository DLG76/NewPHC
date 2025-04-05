using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [Header("PlayerInfoPanel")]
    [SerializeField] private TMP_Text maxHealthText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text minDamageText;
    [SerializeField] private TMP_Text maxDamageText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private Button userIdButton;

    [Header("Overworld")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Slider healthBar;

    private void Awake()
    {
        userIdButton.onClick.RemoveAllListeners();
        userIdButton.onClick.AddListener(() =>
        {
            string myUserId = PlayerPrefs.GetString("myUserId");
            GUIUtility.systemCopyBuffer = myUserId;
        });
    }
    
    private void Update()
    {
        float maxHealth = User.me.maxHealth;
        float health = User.me.health;
        if (User.me.equipment.core != null)
        {
            maxHealth += User.me.equipment.core.health;
            health += User.me.equipment.core.health;
        }
        maxHealthText.text = $"MaxHealth: {maxHealth.ToString("0.00")}";
        healthText.text = $"Health: {health.ToString("0.00")}";

        float armor = 0;
        if (User.me.equipment.core != null)
            armor += User.me.equipment.core.armor;
        armorText.text = $"Armor: {armor.ToString("0.00")}";

        float minDamage = 0;
        if (User.me.equipment.weapon1 != null)
            minDamage += User.me.equipment.weapon1.MinDamage;
        if (User.me.equipment.weapon2 != null)
            minDamage += User.me.equipment.weapon2.MinDamage;
        if (User.me.equipment.weapon3 != null)
            minDamage += User.me.equipment.weapon3.MinDamage;
        minDamageText.text = $"MinDamage: {minDamage.ToString("0.00")}";

        float maxDamage = 0;
        if (User.me.equipment.weapon1 != null)
            maxDamage += User.me.equipment.weapon1.MaxDamage;
        if (User.me.equipment.weapon2 != null)
            maxDamage += User.me.equipment.weapon2.MaxDamage;
        if (User.me.equipment.weapon3 != null)
            maxDamage += User.me.equipment.weapon3.MaxDamage;
        maxDamageText.text = $"MaxDamage: {maxDamage.ToString("0.00")}";

        expText.text = $"Exp: {User.me.exp.ToString("0.00")} / {User.me.maxExp.ToString("0.00")}";

        nameText.text = User.me.name;

        healthBar.maxValue = maxHealth;
        healthBar.value = health;
    }
}
