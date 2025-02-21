using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageTextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float forcePower = 1;
    [SerializeField] private float lifeTime = 0.75f;

    private void Awake()
    {
        damageText.gameObject.SetActive(false);
    }

    public void Setup(float damage)
    {
        if (damage % 1 == 0)
            damageText.text = damage.ToString("0");
        else
            damageText.text = damage.ToString("0.00");

        damageText.gameObject.SetActive(true);

        GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, Random.Range(-25f, 25f)) * Vector2.up * forcePower * 2.5f;

        Destroy(gameObject, lifeTime);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
