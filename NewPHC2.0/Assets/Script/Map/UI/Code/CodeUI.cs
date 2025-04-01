using DG.Tweening;
using Newtonsoft.Json.Linq;
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
    [SerializeField] private Transform descriptionContent;
    [SerializeField] private TMP_InputField exampleOutputField;
    //[SerializeField] private TMP_Text hintText;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private Transform popupsParent;

    [Header("Npc UI")]
    [SerializeField] private Image npcImage;
    [SerializeField] private Transform npcDialogContent;

    [Header("Image UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image characterA;
    [SerializeField] private Image characterB;

    private DialogueManager dialogueManager;
    private Coroutine showCoroutine;

    private TMP_Text descriptionTextModel;
    private Image descriptionImageModel;

    private string stageId;
    private bool sendedCode = false;

    private void Awake()
    {
        storyPanel.SetActive(false);

        submitButton.onClick.AddListener(SendCode);

        descriptionTextModel = Resources.Load<TMP_Text>("UI/DescriptionText");
        descriptionImageModel = Resources.Load<Image>("UI/DescriptionImage");
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
            LoadCodeUI(currentStage);
            codeInputField.interactable = false;
            dialoguePanel.SetActive(false);
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

        LoadCodeUI(currentStage);
        codeInputField.interactable = true;
        submitButton.gameObject.SetActive(true);
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
            string code = codeInputField.text;

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

    private void LoadCodeUI(CodeStage currentStage)
    {
        var npcData = currentStage.StageData["npc"];
        if (npcData != null)
        {
            Sprite npcSprite = Resources.Load<Sprite>($"Sprite/CodeStageSprite/NPC/{npcData["npcPrefabName"]}");

            if (npcSprite != null)
            {
                npcImage.sprite = npcSprite;
                npcImage.GetComponent<AspectRatioFitter>().aspectRatio = npcSprite.textureRect.width / npcSprite.textureRect.height;
                npcImage.gameObject.SetActive(true);

                foreach (Transform c in npcDialogContent)
                    Destroy(c.gameObject);

                foreach (var valueData in npcData["dialog"]?.ToObject<List<JObject>>())
                    if (valueData["valueType"]?.ToString() == "text")
                    {
                        var descriptionText = Instantiate(descriptionTextModel, npcDialogContent);
                        descriptionText.text = valueData["value"]?.ToString();
                    }
                    else if (valueData["valueType"]?.ToString() == "image")
                    {
                        var descriptionImage = Instantiate(descriptionImageModel, npcDialogContent);
                        string spritePath = valueData["value"]?.ToString();
                        if (!string.IsNullOrEmpty(spritePath))
                        {
                            Sprite sprite = Resources.Load<Sprite>($"Sprite/CodeStageSprite/Image/{spritePath}");
                            descriptionImage.sprite = sprite;
                            descriptionImage.GetComponent<AspectRatioFitter>().aspectRatio = sprite.textureRect.width / sprite.textureRect.height;
                        }
                    }
            }
        }
        else npcImage.gameObject.SetActive(false);

        foreach (Transform c in descriptionContent)
            Destroy(c.gameObject);

        foreach (var valueData in currentStage.StageData["description"]?.ToObject<List<JObject>>())
            if (valueData["valueType"]?.ToString() == "text")
            {
                var descriptionText = Instantiate(descriptionTextModel, descriptionContent);
                descriptionText.text = valueData["value"]?.ToString();
            }
            else if (valueData["valueType"]?.ToString() == "image")
            {
                string spritePath = valueData["value"]?.ToString();

                if (!string.IsNullOrEmpty(spritePath))
                {
                    Sprite sprite = Resources.Load<Sprite>($"Sprite/CodeStageSprite/Image/{spritePath}");

                    if (sprite != null)
                    {
                        var descriptionImage = Instantiate(descriptionImageModel, descriptionContent);
                        descriptionImage.sprite = sprite;
                        descriptionImage.GetComponent<AspectRatioFitter>().aspectRatio = sprite.textureRect.width / sprite.textureRect.height;
                    }
                }
            }

        exampleOutputField.text = currentStage.StageData["exampleOutput"]?.ToObject<string>();
        codeInputField.text = currentStage.MyClearedStage?["code"]?.ToObject<string>() ?? string.Empty;

        foreach (var popup in popupsParent.GetComponentsInChildren<WindowPopupUI>())
            popup.QuitWindowNow();

        codePanel.SetActive(true);
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
