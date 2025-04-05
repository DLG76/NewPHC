using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
    private Coroutine playerMoveCoroutine;

    private void Awake()
    {
        StartCoroutine(fadeCanvas.EnterFade());
    }

    private void Start()
    {
        LoadStages(() =>
        {
            Stage.currentStage = PlayerPrefs.GetString("currentStage", null);

            if (!string.IsNullOrEmpty(Stage.currentStage))
            {
                Stage stage = stageObjs.FirstOrDefault(s => s.stageId == Stage.currentStage && !s.isLock && s.StageData != null);

                if (stage != null)
                {
                    player.position = stage.transform.position;
                    return;
                }
            }

            Stage startStage = stageObjs.FirstOrDefault(s => s.isStartStage);
            if (startStage != null)
            {
                Stage.currentStage = startStage.stageId;
                player.position = startStage.transform.position;
            }
        });
    }
    
    private void Update()
    {
        if (!CanEnterStage()) return;

        Stage stage = null;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 100;

            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector3.forward, Mathf.Infinity);

            foreach (RaycastHit2D hit in hits)
                if (hit.transform.gameObject.TryGetComponent(out stage))
                    break;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
            stage = stageObjs.FirstOrDefault(s => s.stageId == Stage.currentStage);

        if (stage != null)
            stage.Select();
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
                Debug.Log("Failed to get stages.");
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

                var allConnectingStages = stageObjs.SelectMany(s => s.ConnectingStages).Distinct().ToArray();

                foreach (var stageObj in stageObjs)
                {
                    var connectingStages = allConnectingStages.Where(cs => cs.stageA == stageObj || cs.stageB == stageObj).ToArray();
                    stageObj.ConnectingStages = connectingStages;
                }

                foreach (var stageObj in clearedStageObjs)
                    UnlockConnectingStages(stageObj);

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

    private void UnlockConnectingStages(Stage stageObj)
    {
        if (stageObj.ConnectingStages == null) return;

        foreach (var nextStageLine in stageObj.ConnectingStages)
            if (nextStageLine != null)
            {
                if (nextStageLine.stageB == stageObj)
                    nextStageLine.stageA?.Unlock();
                if (nextStageLine.stageA == stageObj)
                    nextStageLine.stageB?.Unlock();
            }
    }

    public void PlayerMoveTo(Stage stage)
    {
        if (playerMoveCoroutine != null)
        {
            StopCoroutine(playerMoveCoroutine);
            playerMoveCoroutine = null;
        }

        playerMoveCoroutine = StartCoroutine(PlayerMoveToIE(stage, playerMoveTime));
    }

    public IEnumerator PlayerMoveToIE(Stage stageToGo, float time)
    {
        Stage currentStageObj = stageObjs.FirstOrDefault(s => s.stageId == Stage.currentStage);

        if (currentStageObj == null) yield break;

        if (currentStageObj.ConnectingStages.FirstOrDefault(cs => cs.stageA == stageToGo || cs.stageB == stageToGo) != null)
            yield return PlayerMoveStepToIE(stageToGo, time);

        //StagePathFinder finder = new StagePathFinder();
        ////List<Stage> path = finder.ShortestPathLCA(currentStageObj, stageToGo);
        //List<Stage> path = finder.ShortestPathFromStage(currentStageObj, stageToGo);

        //foreach (var stage in path)
        //    Debug.Log("Stage: " + stage.name);

        //if (path.Count > 0)
        //{
        //    Stage startMoveStage = path[0];
        //    if (Vector2.Distance(startMoveStage.transform.position, player.position) > 0.5)
        //        yield return PlayerMoveStepToIE(startMoveStage, time);

        //    path.RemoveAt(0);
        //}

        //foreach (var stage in path)
        //    yield return PlayerMoveStepToIE(stage, time);
    }

    public IEnumerator PlayerMoveStepToIE(Stage stage, float time)
    {
        var direction = stage.transform.position - player.position;
        direction.Normalize();

        if (direction.x > 0)
            player.GetComponent<SpriteRenderer>().flipX = true;
        else if (direction.x < 0)
            player.GetComponent<SpriteRenderer>().flipX = false;

        Stage.currentStage = stage.stageId;

        yield return player.DOMove(stage.transform.position, time).WaitForCompletion();
    }

    public void PlayCombat(Dungeon dungeon)
    {
        DungeonManager.beforeScene = SceneManager.GetActiveScene().name;
        DungeonManager.dungeon = dungeon;
        StartCoroutine(fadeCanvas.ExitFade("Dungeon"));
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetString("currentStage", Stage.currentStage);
        PlayerPrefs.Save();
    }
}

//public class StagePathFinder
//{
//    // หาทางไปยัง Root (หรือบรรพบุรุษทั้งหมด)
//    public List<Stage> FindPathToRoot(Stage stage)
//    {
//        List<Stage> path = new List<Stage>();
//        while (stage != null)
//        {
//            path.Add(stage);
//            stage = stage.PreviewStage;
//        }
//        path.Reverse(); // กลับลำดับให้เริ่มจาก Root
//        return path;
//    }

//    // หาบรรพบุรุษร่วมที่ใกล้ที่สุด (LCA)
//    public Stage FindLCA(Stage A, Stage B)
//    {
//        List<Stage> pathA = FindPathToRoot(A);
//        List<Stage> pathB = FindPathToRoot(B);

//        Stage lca = null;
//        int minLength = Mathf.Min(pathA.Count, pathB.Count);

//        for (int i = 0; i < minLength; i++)
//        {
//            if (pathA[i] == pathB[i])
//            {
//                lca = pathA[i];
//            }
//            else
//            {
//                break;
//            }
//        }
//        return lca; // คืนค่า LCA
//    }

//    // หาทางที่สั้นที่สุดระหว่าง Stage A → B
//    public List<Stage> ShortestPathLCA(Stage A, Stage B)
//    {
//        Stage lca = FindLCA(A, B);

//        // หาทางจาก A ไปยัง LCA
//        List<Stage> pathAtoLCA = new List<Stage>();
//        Stage current = A;
//        while (current != lca)
//        {
//            pathAtoLCA.Add(current);
//            current = current.PreviewStage;
//        }
//        pathAtoLCA.Add(lca);  // รวม LCA ด้วย

//        // หาทางจาก LCA ไปยัง B
//        List<Stage> pathBtoLCA = new List<Stage>();
//        current = B;
//        while (current != lca)
//        {
//            pathBtoLCA.Add(current);
//            current = current.PreviewStage;
//        }

//        pathBtoLCA.Reverse(); // กลับลำดับให้เป็น LCA → B

//        // รวมเส้นทางจาก A → LCA และ LCA → B
//        pathAtoLCA.AddRange(pathBtoLCA);
//        return pathAtoLCA;
//    }
//}

public class StagePathFinder
{
    // หาทางที่สั้นที่สุดจาก Stage A ไปยัง Stage B โดยใช้ BFS
    public List<Stage> ShortestPathFromStage(Stage startStage, Stage endStage)
    {
        // ถ้าจุดเริ่มต้นและปลายทางเหมือนกัน
        if (startStage == endStage)
            return new List<Stage> { startStage };

        // คิวสำหรับ BFS และแผนที่สำหรับติดตามที่เคยเดินผ่าน
        Queue<Stage> queue = new Queue<Stage>();
        Dictionary<Stage, Stage> parentMap = new Dictionary<Stage, Stage>();

        // เริ่มต้น BFS จาก startStage
        queue.Enqueue(startStage);
        parentMap[startStage] = null; // ไม่มี parent สำหรับ startStage

        // BFS
        while (queue.Count > 0)
        {
            Stage current = queue.Dequeue();

            // ถ้าค้นพบปลายทาง
            if (current == endStage)
                break;

            // ตรวจสอบทุกเส้นทางที่เชื่อมโยงกับ current
            foreach (var connection in current.ConnectingStages)
            {
                Stage neighbor = connection.stageB;

                if (neighbor != null)
                    if (!parentMap.ContainsKey(neighbor)) // ถ้ายังไม่เคยมาเยือน
                    {
                        queue.Enqueue(neighbor);
                        parentMap[neighbor] = current; // บันทึก parent ของ neighbor
                    }
            }
        }

        // สร้างเส้นทางจาก endStage ไปยัง startStage โดยย้อนกลับจาก parentMap
        List<Stage> path = new List<Stage>();
        Stage pathStage = endStage;

        while (pathStage != null)
        {
            path.Insert(0, pathStage); // เพิ่ม stage ในลำดับที่เริ่มจาก startStage
            pathStage = parentMap.ContainsKey(pathStage) ? parentMap[pathStage] : null;
        }

        return path.Count > 1 ? path : new List<Stage>(); // ถ้า path เป็นแค่ startStage ก็คืนค่าว่าง
    }
}
