using UnityEngine;

public class CodeStage : Stage
{
    public SeceneDialogue BeforeSeceneDialogue { get => _beforeSeceneDialogue; }
    [SerializeField] private SeceneDialogue _beforeSeceneDialogue;
    public Quest Quest { get => _quest; }
    [SerializeField] private Quest _quest;
    public SeceneDialogue AfterSeceneDialogue { get => _afterSeceneDialogue; }
	[SerializeField] private SeceneDialogue _afterSeceneDialogue;

    public override void Enter()
    {
        if (isSuccess) return;

        DialogueManager.GetInstance().EnterDialogueMode(_beforeSeceneDialogue.inkJSON, null);
        CodeUI.Instance.Show(this);
    }

    protected override void Update()
    {
        base.Update();
    }
}
