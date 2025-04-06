using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboText : Singleton<ComboText>
{
    [SerializeField] private Gradient gradient;
    [SerializeField] private float maxColorComboDamage = 30000;
    [SerializeField] private float resetComboTime = 5;

    [SerializeField] private TMP_Text comboText;
    [SerializeField] private Slider comboTimeSlider;

    private float comboDamage;
    private float comboTime;

    private List<GameObject> childs;

    private void Awake()
    {
        comboDamage = 0;

        childs = new List<GameObject>();

        foreach (Transform child in transform)
            childs.Add(child.gameObject);
    }

    private void Update()
    {
        comboTimeSlider.maxValue = resetComboTime;
        comboTimeSlider.value = resetComboTime - (Time.time - comboTime);

        if (Time.time - comboTime >= resetComboTime)
            ResetCombo();

        foreach (GameObject child in childs)
            child.SetActive(comboDamage > 0);
    }

    public void AddCombo(float damage)
    {
        comboDamage += damage;
        comboTime = Time.time;

        comboText.text = Mathf.Ceil(comboDamage).ToString();
        comboText.color = GetColorAtPosition();
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
        transform.localScale = Vector3.one * 1.3f;
        transform.DOScale(Vector2.one, 0.5f);
    }

    private void ResetCombo()
    {
        comboDamage = 0;
    }

    private Color GetColorAtPosition()
    {
        float position = comboDamage / maxColorComboDamage;
        position = Mathf.Clamp01(position);
        return gradient.Evaluate(position);
    }
}
