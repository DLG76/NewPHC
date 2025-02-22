using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    private static List<string> unlockStages = new List<string>();

    public IEnumerator FinishedDungeon(Dungeon dungeon)
    {
        var stage = dungeon.Stage;

        yield break;
    }

    public IEnumerator SuccessStage(Stage stage)
    {
        yield break;
    }

    public IEnumerator SendCode(Stage stage, string code)
    {
        stage.Success();
        StageManager.unlockStages.Add(stage.stageName);
        StageManager.unlockStages.Add(stage.stageName);
        unlockStages.Add(stage.stageName);
        yield break;
    }
}
