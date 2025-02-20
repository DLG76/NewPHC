using UnityEngine;

public class CombatStage : Stage
{
    public override void Enter()
    {
        Debug.Log("Go To Combat");
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
