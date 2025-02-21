using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WallStoneTrab : MonoBehaviour, ICombatEntity
{
    [SerializeField] private float destroyTime = 5;
    [SerializeField] private float health = 100;
    [SerializeField] private TMP_Text timeText;

    private List<PlayerCombat> playerInArea = new List<PlayerCombat>();

    private void Awake()
    {
        timeText.text = destroyTime.ToString("0.00s");
        timeText.DOColor(Color.red, destroyTime);
    }

    private void Update()
    {
        timeText.text = destroyTime.ToString("0.00s");

        if (destroyTime <= 0)
        {
            if (playerInArea.Count > 0)
            {
                foreach (var cha in ServerManager.Characters)
                {
                    cha.TakeDamage(cha.Health / 4f);
                }
            }
            Destroy(gameObject);
        }
        else destroyTime -= Time.deltaTime;
    }

    public void Heal(float heal)
    {

    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float dmg)
    {
        health = Mathf.Max(health - dmg, 0);

        if (health <= 0)
            Die();
    }

    public void ApplyForce(Vector2 vector, float power)
    {

    }

    public T AddStatusEffect<T>() where T : StatusEffect
    {
        return null;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerCombat player))
        {
            if (!playerInArea.Contains(player))
            {
                playerInArea.Add(player);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerCombat player))
        {
            if (playerInArea.Contains(player))
            {
                playerInArea.Remove(player);
            }
        }
    }
}
