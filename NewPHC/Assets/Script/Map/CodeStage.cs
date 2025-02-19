using UnityEngine;

public class CodeStage : Stage
{
    public override void Enter()
    {
        Debug.Log("Go To Code");
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.E) && currentStage == stageName)
        {
            Success();
        }
    }
}
