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

    private void Start()
    {
        var player = PlayerCombat.LocalPlayerInstance;

        if (player != null)
        {
            healthBar.maxValue = player.MaxHealth;
            healthBar.value = player.Health;
            healthUI.SetActive(true);
        }
        else
        {
            healthUI.SetActive(false);
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
        }
        else
        {
            healthUI.SetActive(false);
        }
    }
}
