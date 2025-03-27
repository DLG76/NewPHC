using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [Header("Profile Panel")]
    [SerializeField] private GameObject profilePanel;
    [Header("Stage Data")]
    [SerializeField] private Transform stagesParent;
    [SerializeField] private Transform player;
    [SerializeField] private float playerMoveTime = 0.75f;
    [SerializeField] private FadeCanvas fadeCanvas;

    private Stage[] stageObjs;

    private void Awake()
    {
        Stage.currentStage = PlayerPrefs.GetString("currentStage", null);

        StartCoroutine(fadeCanvas.EnterFade());
    }

    private void Start()
    {
        LoadStages(() =>
        {
            if (!string.IsNullOrEmpty(Stage.currentStage))
            {
                Stage stage = stageObjs.FirstOrDefault(s => s.stageId == Stage.currentStage && s.MyClearedStage != null);

                if (stage != null)
                {
                    player.position = stage.transform.position;
                }
                else
                {
                    Stage startStage = stageObjs.FirstOrDefault(s => s.isStartStage);
                    if (startStage != null)
                    {
                        Stage.currentStage = startStage.stageId;
                        player.position = startStage.transform.position;
                    }
                }
            }
        });
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 100;

            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, transform.right, Mathf.Infinity);

            foreach (RaycastHit2D hit in hits)
                if (hit.transform.gameObject.TryGetComponent(out Stage stage))
                {
                    stage.Select();
                    break;
                }
        }
    }

    public void LoadStages() => LoadStages(null);

    public void LoadStages(System.Action onStagesLoaded)
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

                onStagesLoaded?.Invoke();
            }));
        }));
    }

    public bool CanEnterStage()
    {
        if (profilePanel == null)
            return true;

        return !profilePanel.activeSelf;
    }

    private void UnlockNextStages(Stage stageObj)
    {
        foreach (var nextStageObj in stageObj.NextStages)
            nextStageObj.Unlock(stageObj);
    }

    public void PlayerMoveTo(Stage stage) => PlayerMoveTo(stage, playerMoveTime);

    public void PlayerMoveTo(Stage stage, float time)
    {
        var direction = stage.transform.position - player.position;
        direction.Normalize();

        if (direction.x > 0)
            player.GetComponent<SpriteRenderer>().flipX = true;
        else if (direction.x < 0)
            player.GetComponent<SpriteRenderer>().flipX = false;

        player.DOMove(stage.transform.position, time);
    }

    public void PlayCombat(Dungeon dungeon)
    {
        DungeonManager.dungeon = dungeon;
        StartCoroutine(fadeCanvas.ExitFade("Dungeon"));
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetString("currentStage", Stage.currentStage);
        PlayerPrefs.Save();
    }
}
