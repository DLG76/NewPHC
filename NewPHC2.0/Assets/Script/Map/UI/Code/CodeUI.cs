using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor.Rendering;
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
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private Transform npcDialogContent;

    [Header("Image UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image characterA;
    [SerializeField] private Image characterB;

    private DialogueManager dialogueManager;
    private Coroutine showCoroutine;

    private TMP_Text descriptionTextModel;
    private TMP_Text dialogTextModel;
    private Image imageModel;

    private string stageId;
    private bool sendedCode = false;

    private void Awake()
    {
        storyPanel.SetActive(false);

        submitButton.onClick.AddListener(SendCode);

        descriptionTextModel = Resources.Load<TMP_Text>("UI/DescriptionText");
        dialogTextModel = Resources.Load<TMP_Text>("UI/DialogText");
        imageModel = Resources.Load<Image>("UI/Image");
    }

    private void Start()
    {
        dialogueManager = DialogueManager.GetInstance();

        characterA.color = new Color32(150, 150, 150, 255);
        characterB.color = new Color32(150, 150, 150, 255);

        if (dialogueManager != null)
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

            yield return DatabaseManager.Instance.SendCode(stageId, code, (success, reward) =>
            {
                if (success)
                {
                    storyPanel.SetActive(false);
                    StageManager.Instance.LoadStages();
                    RewardUI.CreateRewardUI(reward);
                }

                sendedCode = true;
            });
        }
    }

    private void LoadCodeUI(CodeStage currentStage)
    {
        var npcData = currentStage.StageData["npc"];
        if (npcData != null && npcData.Type != JTokenType.Null)
        {
            Sprite npcSprite = Resources.Load<Sprite>($"Sprite/CodeStageSprite/NPC/{npcData["npcPrefabName"]}");

            if (npcSprite != null)
            {
                npcImage.sprite = npcSprite;
                npcImage.GetComponent<AspectRatioFitter>().aspectRatio = npcSprite.rect.height / npcSprite.rect.width;
                npcNameText.text = npcData["npcPrefabName"]?.ToString();
                npcImage.gameObject.SetActive(true);

                foreach (Transform c in npcDialogContent)
                    Destroy(c.gameObject);

                foreach (var valueData in npcData["dialog"]?.ToObject<List<JObject>>())
                    if (valueData["valueType"]?.ToString() == "text")
                    {
                        var dialogText = Instantiate(dialogTextModel, npcDialogContent);
                        dialogText.text = GetText(valueData["value"]?.ToString());
                    }
                    else if (valueData["valueType"]?.ToString() == "image")
                    {
                        string spritePath = valueData["value"]?.ToString();
                        if (!string.IsNullOrEmpty(spritePath))
                        {
                            Sprite sprite = Resources.Load<Sprite>($"Sprite/CodeStageSprite/Image/{spritePath}");

                            if (sprite != null)
                            {
                                var image = Instantiate(imageModel, npcDialogContent);
                                image.sprite = sprite;
                                image.GetComponent<AspectRatioFitter>().aspectRatio = sprite.rect.width / sprite.rect.height;
                            }
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
                descriptionText.text = GetText(valueData["value"]?.ToString());
            }
            else if (valueData["valueType"]?.ToString() == "image")
            {
                string spritePath = valueData["value"]?.ToString();

                if (!string.IsNullOrEmpty(spritePath))
                {
                    Sprite sprite = Resources.Load<Sprite>($"Sprite/CodeStageSprite/Image/{spritePath}");

                    if (sprite != null)
                    {
                        var image = Instantiate(imageModel, descriptionContent);
                        image.sprite = sprite;
                        image.GetComponent<AspectRatioFitter>().aspectRatio = sprite.rect.width / sprite.rect.height;
                    }
                }
            }

        exampleOutputField.text = currentStage.StageData["exampleOutput"]?.ToObject<string>() ?? string.Empty;
        codeInputField.text = currentStage.MyClearedStage?["code"]?.ToObject<string>() ?? string.Empty;

        foreach (var popup in popupsParent.GetComponentsInChildren<WindowPopupUI>())
            popup.QuitWindowNow();

        codePanel.SetActive(true);
    }

    private string GetText(string defaultText)
    {
        // Match คำหรือ delimiter (space หรือ newline)
        var matches = Regex.Matches(defaultText, @"[^\s\r\n]+|[\r\n]+|[ ]+");
        var result = new List<string>();

        foreach (Match match in matches)
        {
            string token = match.Value;

            if (token.StartsWith("http"))
            {
                // Rich link: ไม่มีการ escape
                var url = token;
                result.Add($"<link=\"{url}\"><color=#05C2FF><u>{url}</u></color></link>");
            }
            else if (token == " " || token == "\r" || token == "\n" || Regex.IsMatch(token, @"^[\r\n ]+$"))
            {
                // เก็บช่องว่างและ newline ตามเดิม
                result.Add(token.Replace("\r\n", "\n").Replace("\r", "\n")); // Normalize ให้หมดเป็น \n
            }
        }

        return string.Concat(result);
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
