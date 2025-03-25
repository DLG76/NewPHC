using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatStage : Stage
{
    private Dungeon dungeon;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Setup(JObject stageData, JObject myClearedStage)
    {
        base.Setup(stageData, myClearedStage);

        if (stageData != null)
            dungeon = Dungeon.Parse(_stageData["dungeon"]?.ToObject<JObject>());
    }

    public override void Success()
    {

    }

    public override void Enter()
    {
        StageManager.Instance.PlayCombat(dungeon);
    }

    protected override void Update()
    {
        base.Update();
    }
}
