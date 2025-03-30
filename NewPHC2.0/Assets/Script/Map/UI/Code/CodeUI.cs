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
    [SerializeField] private GameObject storyPanel;

    [Header("Code UI")]
    [SerializeField] private GameObject codePanel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_InputField exampleOutputText;
    //[SerializeField] private TMP_Text hintText;
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private Button submitButton;

    [Header("Image UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image characterA;
    [SerializeField] private Image characterB;

    private DialogueManager dialogueManager;
    private Coroutine showCoroutine;

    private string stageId;
    private bool sendedCode = false;

    private void Awake()
    {
        storyPanel.SetActive(false);

        submitButton.onClick.AddListener(SendCode);
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

    public void Show(CodeStage currentStage, string stageId)
    {
        this.stageId = stageId;

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        showCoroutine = StartCoroutine(ShowIE(currentStage));
    }

    private IEnumerator ShowIE(CodeStage currentStage)
    {
        if (currentStage.MyClearedStage != null)
        {
            descriptionText.text = currentStage.StageData["description"]?.ToObject<string>();
            exampleOutputText.text = currentStage.StageData["exampleOutput"]?.ToObject<string>();
            codeInput.text = currentStage.MyClearedStage["code"]?.ToObject<string>();
            codeInput.interactable = false;
            dialoguePanel.SetActive(false);
            codePanel.SetActive(true);
            submitButton.gameObject.SetActive(false);
            storyPanel.SetActive(true);
            yield break;
        }

        sendedCode = false;

        codePanel.SetActive(false);
        storyPanel.SetActive(true);

        var beforeSeceneDialogue = currentStage.BeforeSeceneDialogue;
        var afterSeceneDialogue = currentStage.AfterSeceneDialogue;

        if (beforeSeceneDialogue != null && beforeSeceneDialogue.inkJSON != null)
        {
            dialoguePanel.SetActive(true);
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
        else dialoguePanel.SetActive(false);

        descriptionText.text = currentStage.StageData["description"]?.ToObject<string>();
        exampleOutputText.text = currentStage.StageData["exampleOutput"]?.ToObject<string>();
        //hintText.text = quest.hint;
        submitButton.gameObject.SetActive(true);
        codeInput.interactable = true;
        codePanel.SetActive(true);

        while (!sendedCode)
            yield return null;

        if (afterSeceneDialogue != null && afterSeceneDialogue.inkJSON != null)
        {
            dialoguePanel.SetActive(true);
            dialogueManager.EnterDialogueMode(afterSeceneDialogue.inkJSON, null);

            while (dialogueManager.dialogueIsPlaying)
                yield return null;
        }

        storyPanel.SetActive(false);
    }

    public void SendCode() => StartCoroutine(SendCodeIE());

    private IEnumerator SendCodeIE()
    {
        if (!string.IsNullOrEmpty(stageId))
        {
            string code = codeInput.text;

            yield return DatabaseManager.Instance.SendCode(stageId, code, (success) =>
            {
                Debug.Log("Send code: " + success);

                if (success)
                {
                    storyPanel.SetActive(false);
                    StageManager.Instance.LoadStages();
                }

                sendedCode = true;
            });
        }
    }

    public void ExitQuest()
    {
        dialogueManager.StopAllCoroutines();

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        storyPanel.SetActive(false);
    }
}
