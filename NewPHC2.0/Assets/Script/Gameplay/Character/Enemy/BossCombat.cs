using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class BossCombat : EnemyCombat
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private string bossName;

    protected override void Awake()
    {
        base.Awake();

        healthText.text = $"{Mathf.Ceil(_health / _maxHealth * 100)}%";

        nameText.text = bossName.ToUpper();
    }

    protected override void Update()
    {
        enemyCanvas.SetActive(true);
        healthText.text = $"{Mathf.Ceil(_health / _maxHealth * 100)}%";
        healthBar.maxValue = _maxHealth;
        healthBar.value = Mathf.Lerp(healthBar.value, _health, Time.deltaTime * 7.5f);
    }

    public override void Die()
    {
        died = true;

        CameraController.Instance.TriggerShake(0.06f, 1.5f, 0.8f);
        CameraController.Instance.LerpVignetteIntensity(0.4f, 0, 1.5f, new Color32(120, 0, 0, 255));

        base.Die();
    }
}
