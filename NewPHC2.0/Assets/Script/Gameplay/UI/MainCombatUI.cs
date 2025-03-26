using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainCombatUI : MonoBehaviour
{
    [SerializeField] private GameObject healthUI;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider healthBar;

    [SerializeField] private GameObject armorUI;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private Slider armorBar;

    private void Start()
    {
        var player = PlayerCombat.LocalPlayerInstance;

        if (player != null)
        {
            healthBar.maxValue = player.MaxHealth;
            healthBar.value = player.Health;
            healthUI.SetActive(true);

            if (player.MaxArmor > 0)
            {
                armorBar.maxValue = player.MaxArmor;
                armorBar.value = player.Armor;
                armorUI.SetActive(true);
            }
            else
            {
                armorUI.SetActive(false);
            }
        }
        else
        {
            healthUI.SetActive(false);
            armorUI.SetActive(false);
        }
    }

    private void Update()
    {
        var player = PlayerCombat.LocalPlayerInstance;

        if (player != null)
        {
            healthText.text = $"{player.Health} / {player.MaxHealth}";
            healthBar.maxValue = player.MaxHealth;
            if (!healthUI.activeSelf)
            {
                healthBar.value = player.Health;
            }
            else
            {
                healthBar.value = Mathf.Lerp(healthBar.value, player.Health, Time.deltaTime * 7.5f);
            }
            healthUI.SetActive(true);

            if (player.MaxArmor > 0)
            {
                armorText.text = $"{player.Armor} / {player.MaxArmor}";
                armorBar.maxValue = player.MaxArmor;
                if (!armorUI.activeSelf)
                {
                    armorBar.value = player.Armor;
                }
                else
                {
                    armorBar.value = Mathf.Lerp(armorBar.value, player.Armor, Time.deltaTime * 7.5f);
                }
                armorUI.SetActive(true);
            }
            else
            {
                armorUI.SetActive(false);
            }
        }
        else
        {
            healthUI.SetActive(false);
            armorUI.SetActive(false);
        }
    }
}
