using TMPro;
using UnityEngine;

public class CodeStage : Stage
{
    public SeceneDialogue BeforeSeceneDialogue { get => _beforeSeceneDialogue; }
    [SerializeField] private SeceneDialogue _beforeSeceneDialogue;
    public SeceneDialogue AfterSeceneDialogue { get => _afterSeceneDialogue; }
	[SerializeField] private SeceneDialogue _afterSeceneDialogue;

    protected override void Awake()
    {
        base.Awake();

        var stageText = GetComponentInChildren<TMP_Text>();
        if (stageText != null)
            stageText.text = name;
    }

    public override void Enter()
    {
        if (MyClearedStage == null && _beforeSeceneDialogue.inkJSON)
        {
            DialogueManager.GetInstance().EnterDialogueMode(_beforeSeceneDialogue.inkJSON, null);
        }
        CodeUI.Instance.Show(this, stageId);
    }
}
