using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DungeonManager : Singleton<DungeonManager>
{
    public static string beforeScene;
    public static Dungeon dungeon;

    [SerializeField] private CinemachineVirtualCamera cinemachine;
    [SerializeField] private float animationTime = 8;
    /*public Transform Map { get => _map; }
    [SerializeField] private Transform _map;*/
    [SerializeField] private FadeCanvas fadeCanvas;
    [SerializeField] private Transform enemySpawner;
    [SerializeField] private Vector3 playerSpawnPosition;
    [SerializeField] private int startWave = 1;
    //[SerializeReference] private Wave[] waves;

    public bool IsEndDungeon { get => _isEndDungeon; }
    private bool _isEndDungeon = false;
    public bool IsStartingWave { get => _isStartingWave; }
    private bool _isStartingWave = false;
    private bool isFadeingWave = false;

    private double startTime;

    private void Awake()
    {
        if (dungeon == null)
        {
            ExitDungeon();
            return;
        }

        _isEndDungeon = false;

        startTime = Time.time;
        startWave = Mathf.Max(startWave, 1);

        EnterDungeon();
    }

    private void Update()
    {
        if (dungeon == null)
        {
            ExitDungeon();
            return;
        }

        if (!_isStartingWave)
            StartCoroutine(FadeNextWave());

        else if (ServerManager.Characters.Length == 0)
        {
            ExitDungeon();
        }
    }

    private IEnumerator FadeNextWave()
    {
        if (_isStartingWave || isFadeingWave) yield break;

        isFadeingWave = true;

        var voidObjects = GameObject.FindGameObjectsWithTag("Void").Select(v => v.GetComponent<VoidObject>());

        foreach (var voidObject in voidObjects)
        {
            if (voidObject != null)
            {
                var voidItemDropObj = new GameObject();
                voidItemDropObj.transform.position = voidObject.transform.position;
                var voidItemDrop = voidItemDropObj.AddComponent<VoidItemDrop>();
                voidItemDrop.Setup(voidObject.Item);
                Destroy(voidObject.gameObject);
            }
        }

        var voidItemDrops = FindObjectsOfType<VoidItemDrop>();

        foreach (var voidItemDrop in voidItemDrops)
            voidItemDrop.Collect();

        foreach (var cha in ServerManager.Characters)
            cha.enabled = false;

        if (startWave > dungeon.Waves.Length)
        {
            _isEndDungeon = true;

            yield return new WaitForSeconds(1);

            yield return DatabaseManager.Instance.FinishedDungeon(dungeon.StageData["_id"]?.ToString(), true, Time.time - startTime, (success, reward) =>
            {
                RewardUI.CreateRewardUI(reward, ExitDungeon);
            });

            yield break;
        }

        if (startWave > 1)
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("Object"))
                Destroy(obj);

            foreach (var cha in ServerManager.Characters)
            {
                var chaPos = cha.transform.position;
                playerSpawnPosition.z = chaPos.z;
                cha.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                cha.transform.DOMove(playerSpawnPosition + cha.Offset, 1);
            }

            yield return new WaitForSeconds(2);

            cinemachine.enabled = false;

            foreach (var cha in ServerManager.Characters)
            {
                var chaPos = cha.transform.position;
                playerSpawnPosition.z = chaPos.z;
                cha.transform.DOMove(playerSpawnPosition + cha.Offset + new Vector3(0, 50, 0), animationTime / 2f);
            }

            yield return new WaitForSeconds((animationTime / 2f) + 1);
        }

        foreach (var cha in ServerManager.Characters)
        {
            var chaPos = cha.transform.position;
            playerSpawnPosition.z = chaPos.z;
            cha.transform.position = playerSpawnPosition - new Vector3(0, 50, 0);
            cha.transform.DOMove(playerSpawnPosition + cha.Offset, animationTime / 2f);
        }

        yield return new WaitForSeconds(animationTime / 2f);

        foreach (var cha in ServerManager.Characters)
            cha.enabled = true;

        cinemachine.enabled = true;

        var wave = dungeon.Waves[startWave - 1];

        _isStartingWave = true;
        isFadeingWave = false;

        yield return wave.StartWave(enemySpawner.GetComponent<PolygonCollider2D>());

        startWave++;

        _isStartingWave = false;
    }

    private void EnterDungeon()
    {
        StartCoroutine(fadeCanvas.EnterFade());
    }

    private void ExitDungeon()
    {
        StartCoroutine(fadeCanvas.ExitFade(beforeScene));
    }

    private void OnDestroy()
    {
        dungeon = null;
    }
}

