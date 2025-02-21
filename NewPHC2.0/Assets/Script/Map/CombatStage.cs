using UnityEngine;

public class CombatStage : Stage
{
    [SerializeField] private Dungeon dungeon;
    [SerializeField] private Reward reward;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Success()
    {
        if (isSuccess) return;

        RewardManager.Instance.ClameReward(reward);

        base.Success();
    }

    public override void Enter()
    {
        dungeon.Stage = stageName;
        StageManager.Instance.PlayCombat(dungeon);
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
