using UnityEngine;

public class CodeStage : Stage
{
    public SeceneDialogue BeforeSeceneDialogue { get => _beforeSeceneDialogue; }
    [SerializeField] private SeceneDialogue _beforeSeceneDialogue;
    public SeceneDialogue AfterSeceneDialogue { get => _afterSeceneDialogue; }
	[SerializeField] private SeceneDialogue _afterSeceneDialogue;

    public override void Enter()
    {
        if (MyClearedStage == null && _beforeSeceneDialogue.inkJSON)
        {
            DialogueManager.GetInstance().EnterDialogueMode(_beforeSeceneDialogue.inkJSON, null);
        }
        CodeUI.Instance.Show(this, stageId);
    }
}
