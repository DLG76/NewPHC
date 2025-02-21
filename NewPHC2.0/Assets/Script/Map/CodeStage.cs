using UnityEngine;

public class CodeStage : Stage
{
    [SerializeField] private Quest quest;

    public override void Enter()
    {
        CodeUI.Instance.Show(this, quest);
    }

    protected override void Update()
    {
        base.Update();
    }
}
