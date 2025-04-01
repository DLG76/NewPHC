using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatStage : Stage
{
    [SerializeField] private Dungeon dungeon;

    public override void Setup(JObject stageData, JObject myClearedStage)
    {
        base.Setup(stageData, myClearedStage);

        if (stageData != null)
            dungeon = Dungeon.Parse(_stageData["dungeon"]?.ToObject<JObject>(), stageData);
    }

    public override void Enter()
    {
        StageManager.Instance.PlayCombat(dungeon);
    }
}
