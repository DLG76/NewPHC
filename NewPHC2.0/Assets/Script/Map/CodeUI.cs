using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeUI : Singleton<CodeUI>
{
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text exampleOutputText;
    //[SerializeField] private TMP_Text hintText;
    [SerializeField] private TMP_InputField codeInput;

    private Stage currentStage;

    private void Awake()
    {
        questPanel.SetActive(false);
    }

    public void Show(Stage currentStage, Quest quest)
    {
        descriptionText.text = quest.description;
        exampleOutputText.text = quest.exampleOutput;
        //hintText.text = quest.hint;
        questPanel.SetActive(true);
    }

    public void SendCode()
    {
        DatabaseManager.Instance.SendCode(currentStage, codeInput.text);
        questPanel.SetActive(false);
    }
}
