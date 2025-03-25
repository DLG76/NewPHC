using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [SerializeField] private Transform stagesParent;
    [SerializeField] private Transform player;
    [SerializeField] private float playerMoveTime = 0.75f;
    [SerializeField] private FadeCanvas fadeCanvas;

    private Stage[] stageObjs;

    private void Awake()
    {
        LoadStages();

        if (!string.IsNullOrEmpty(Stage.currentStage))
        {
            Stage stage = stageObjs.FirstOrDefault(s => s.stageId == Stage.currentStage);

            if (stage != null)
            {
                PlayerMoveTo(stage, 0);
            }
        }

        StartCoroutine(fadeCanvas.EnterFade());
    }

    private void Update()
    {

    }

    public void LoadStages()
    {
        if (stagesParent == null)
        {
            Debug.LogError("stagesParent is null");
            return;
        }

        stageObjs = stagesParent.GetComponentsInChildren<Stage>();

        StartCoroutine(DatabaseManager.Instance.GetStages((success, myClearedStages, stages) =>
        {
            if (!success)
            {
                DatabaseManager.Instance.GoToLoginScene();
                return;
            }

            StartCoroutine(DatabaseManager.Instance.GetProfile((success, profile) =>
            {
                if (!success)
                {
                    Debug.LogError("Failed to get profile.");
                    DatabaseManager.Instance.GoToLoginScene();
                    return;
                }

                if (profile == null)
                {
                    Debug.LogError("Failed to get profile.");
                    return;
                }

                User.me.UpdateProfile(profile);

                List<Stage> clearedStageObjs = new List<Stage>();

                foreach (var stageObj in stageObjs)
                {
                    if (!string.IsNullOrEmpty(stageObj.stageId))
                    {
                        JObject stageData = stages.FirstOrDefault(s => s["_id"]?.ToString() == stageObj.stageId);
                        JObject myClearedStage = myClearedStages.FirstOrDefault(s => s["stageId"]?.ToString() == stageObj.stageId);
                        if (myClearedStage != null && myClearedStage["type"]?.ToString() == "CodeStage")
                            myClearedStage["code"] = User.me.answers.FirstOrDefault(a => a.stageId == stageObj.stageId)?.code;

                        stageObj.Setup(stageData, myClearedStage);

                        if (myClearedStage != null)
                            clearedStageObjs.Add(stageObj);
                    }
                    else stageObj.Setup(null, null);
                }

                foreach (var stageObj in clearedStageObjs)
                    UnlockNextStages(stageObj);
            }));
        }));
    }

    private void UnlockNextStages(Stage stageObj)
    {
        foreach (var nextStageObj in stageObj.NextStages)
            nextStageObj.Unlock();
    }

    public void PlayerMoveTo(Stage stage) => PlayerMoveTo(stage, playerMoveTime);

    public void PlayerMoveTo(Stage stage, float time)
    {
        player.DOMove(stage.transform.position, time);
    }

    public void PlayCombat(Dungeon dungeon)
    {
        DungeonManager.dungeon = dungeon;
        StartCoroutine(fadeCanvas.ExitFade("Dungeon"));
    }
}
