using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    public static string successStage;
    public static List<string> unlockStages = new List<string>();
    private static List<string> successStages = new List<string>();

    [SerializeField] private Stage[] allStages;
    [SerializeField] private Transform landmark;
    [SerializeField] private float landmarkMoveTime = 0.75f;
    [SerializeField] private FadeCanvas fadeCanvas;

    private void Awake()
    {
        LoadStages();

        if (!string.IsNullOrEmpty(Stage.currentStage))
        {
            Stage stage = allStages.FirstOrDefault(s => s.stageName == Stage.currentStage);

            if (stage != null)
            {
                LandmarkMoveTo(stage, 0);
            }
        }

        StartCoroutine(fadeCanvas.EnterFade());
    }

    private void LoadStages()
    {
        // ªÑéèÇ¤ÃÒÇ

        if (!string.IsNullOrEmpty(successStage))
        {
            successStages.Add(successStage);
            var stage = allStages.FirstOrDefault(s => s.stageName == successStage);
            if (stage)
                stage.Success();
        }

        foreach (var stage in allStages)
        {
            stage.Setup(
                unlockStages.Find(s => stage.stageName == s) == null,
                successStages.Find(s => stage.stageName == s) != null
            );
        }
    }

    private void SaveStages()
    {

    }

    public void LandmarkMoveTo(Stage stage) => LandmarkMoveTo(stage, landmarkMoveTime);

    public void LandmarkMoveTo(Stage stage, float time)
    {
        landmark.DOMove(stage.transform.position, time);
    }

    public void PlayCombat(Dungeon dungeon)
    {
        DungeonManager.dungeon = dungeon;
        StartCoroutine(fadeCanvas.ExitFade("Dungeon"));
    }
}
