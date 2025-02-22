using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CodeUI : Singleton<CodeUI>
{
    [SerializeField] private GameObject questPanel;

    [Header("Code UI")]
    [SerializeField] private GameObject codePanel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text exampleOutputText;
    //[SerializeField] private TMP_Text hintText;
    [SerializeField] private TMP_InputField codeInput;

    [Header("Image UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image characterA;
    [SerializeField] private Image characterB;

    private DialogueManager dialogueManager;
    private Stage currentStage;
    private Coroutine showCoroutine;

    private bool sendedCode = false;

    private void Awake()
    {
        questPanel.SetActive(false);
    }

    private void Start()
    {
        dialogueManager = DialogueManager.GetInstance();

        characterA.color = new Color32(150, 150, 150, 255);
        characterB.color = new Color32(150, 150, 150, 255);

        dialogueManager.onCharacterTalk += (character) =>
        {
            if (character == characterA.name)
            {
                characterA.DOColor(Color.white, 0.3f);
                characterB.DOColor(new Color32(150, 150, 150, 255), 0.3f);
            }
            else if (character == characterB.name)
            {
                characterA.DOColor(new Color32(150, 150, 150, 255), 0.3f);
                characterB.DOColor(Color.white, 0.3f);
            }
        };
    }

    public void Show(CodeStage currentStage)
    {
        this.currentStage = currentStage;

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        showCoroutine = StartCoroutine(ShowIE(currentStage));
    }

    private IEnumerator ShowIE(CodeStage currentStage)
    {
        sendedCode = false;

        codePanel.SetActive(false);
        dialoguePanel.SetActive(true);
        questPanel.SetActive(true);

        var beforeSeceneDialogue = currentStage.BeforeSeceneDialogue;
        var afterSeceneDialogue = currentStage.AfterSeceneDialogue;

        if (beforeSeceneDialogue != null && beforeSeceneDialogue.inkJSON != null)
        {
            bgImage.sprite = beforeSeceneDialogue.bg;
            characterA.name = beforeSeceneDialogue.characterDialogueA.name;
            characterA.sprite = beforeSeceneDialogue.characterDialogueA.img;
            characterB.name = beforeSeceneDialogue.characterDialogueB.name;
            characterB.sprite = beforeSeceneDialogue.characterDialogueB.img;

            dialogueManager.EnterDialogueMode(beforeSeceneDialogue.inkJSON, null);

            while (dialogueManager.dialogueIsPlaying)
                yield return null;

            Debug.Log(dialogueManager.dialogueIsPlaying);
        }

        descriptionText.text = currentStage.Quest.description;
        exampleOutputText.text = currentStage.Quest.exampleOutput;
        //hintText.text = quest.hint;
        codePanel.SetActive(true);

        while (!sendedCode)
            yield return null;

        if (afterSeceneDialogue != null && afterSeceneDialogue.inkJSON != null)
        {
            dialogueManager.EnterDialogueMode(afterSeceneDialogue.inkJSON, null);

            while (dialogueManager.dialogueIsPlaying)
                yield return null;
        }

        questPanel.SetActive(false);
    }

    public void SendCode() => StartCoroutine(SendCodeIE());

    private IEnumerator SendCodeIE()
    {
        yield return DatabaseManager.Instance.SendCode(currentStage, codeInput.text);
        questPanel.SetActive(false);
        sendedCode = true;
    }

    public void ExitQuest()
    {
        dialogueManager.StopAllCoroutines();

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        questPanel.SetActive(false);
    }
}
