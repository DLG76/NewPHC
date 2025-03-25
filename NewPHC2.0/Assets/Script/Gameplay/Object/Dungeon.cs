using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Dungeon
{
    [System.Serializable]
    public class Wave
    {
        public EnemySpawnData[] enemySpawnDatas;

        public virtual IEnumerator StartWave(PolygonCollider2D spawnCollider)
        {
            var allEnemies = new List<EnemyCombat>();

            foreach (var enemySpawnData in enemySpawnDatas)
            {
                var enemies = enemySpawnData.SpawnEnemy(spawnCollider);

                allEnemies.AddRange(enemies);
            }

            while (true)
            {
                if (allEnemies.Where(e => e != null && e.Health > 0).FirstOrDefault() == null)
                    break;

                yield return null;
            }

            yield return new WaitForSeconds(2.5f);
        }
    }

    [System.Serializable]
    public class BossWave : Wave
    {
        public BossCombat BossModel { get => bossModel; set => bossModel = value; }
        public Vector3 BossPos { get => bossPos; set => bossPos = value; }
        [SerializeField] private BossCombat bossModel;
        [SerializeField] private Vector3 bossPos;

        public override IEnumerator StartWave(PolygonCollider2D spawnCollider)
        {
            if (enemySpawnDatas.Length > 0)
                yield return base.StartWave(spawnCollider);

            if (enemySpawnDatas.Length == 0)
                yield return new WaitForSeconds(2.5f);

            var boss = Object.Instantiate(bossModel, spawnCollider.transform);
            boss.transform.position = bossPos;

            while (true)
            {
                if (boss == null || boss.Health <= 0)
                    break;

                yield return null;
            }

            yield return new WaitForSeconds(5);
        }
    }

    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemyCombat EnemyModel { get => enemyModel; set => enemyModel = value; }
        public int Count { get => count; set => count = value; }
        [SerializeField] private EnemyCombat enemyModel;
        [SerializeField] private int count = 1;
        private PolygonCollider2D spawnCollider;

        public EnemyCombat[] SpawnEnemy(PolygonCollider2D spawnCollider)
        {
            this.spawnCollider = spawnCollider;

            var enemies = new List<EnemyCombat>();

            for (int _ = 0; _ < count; _++)
            {
                var enemy = Object.Instantiate(enemyModel, spawnCollider.transform);
                enemy.transform.position = RandomSpawnPosition();

                enemies.Add(enemy);
            }

            return enemies.ToArray();
        }

        private Vector2 RandomSpawnPosition()
        {
            if (spawnCollider == null || spawnCollider.points.Length == 0) return Vector3.zero;

            Vector2 randomPoint;
            float minimumDistance = 5f;

            do
            {
                Vector2 min = spawnCollider.bounds.min;
                Vector2 max = spawnCollider.bounds.max;
                randomPoint = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            } while (!IsPointInPolygon(randomPoint, spawnCollider.points, spawnCollider.transform)
                     || !IsFarEnoughFromPlayers(randomPoint, minimumDistance));

            return randomPoint;
        }

        private bool IsFarEnoughFromPlayers(Vector2 point, float minimumDistance)
        {
            foreach (var character in ServerManager.Characters)
            {
                Vector2 playerPosition = character.transform.position;

                if (Vector2.Distance(point, playerPosition) < minimumDistance)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsPointInPolygon(Vector2 point, Vector2[] pPoints, Transform pTransform)
        {
            Vector2 localPoint = pTransform.InverseTransformPoint(point);
            int j = pPoints.Length - 1;
            bool inside = false;

            for (int i = 0; i < pPoints.Length; i++)
            {
                if (pPoints[i].y < localPoint.y && pPoints[j].y >= localPoint.y || pPoints[j].y < localPoint.y && pPoints[i].y >= localPoint.y)
                {
                    if (pPoints[i].x + (localPoint.y - pPoints[i].y) / (pPoints[j].y - pPoints[i].y) * (pPoints[j].x - pPoints[i].x) < localPoint.x)
                    {
                        inside = !inside;
                    }
                }
                j = i;
            }

            return inside;
        }

    }

    public Wave[] Waves { get => _waves; }
    [SerializeReference] private Wave[] _waves;
    public JObject StageData { get; private set; }

    private static Vector3 ParseVector3(JToken positionJson)
    {
        if (positionJson == null || positionJson.Type == JTokenType.Null)
            return Vector3.zero;

        return new Vector3(
            positionJson["x"]?.ToObject<float>() ?? 0f,
            positionJson["y"]?.ToObject<float>() ?? 0f,
            0f
        );
    }

    private static BossCombat GetBossModel(string bossPrefabName)
    {
        return Resources.Load<BossCombat>($"Dungeon/Enemy/Boss/{bossPrefabName}");
    }

    private static EnemyCombat GetEnemyModel(string enemyPrefabName)
    {
        return Resources.Load<EnemyCombat>($"Dungeon/Enemy/{enemyPrefabName}");
    }

    private static EnemySpawnData[] ParseEnemySpawnDatas(JToken enemySpawnDatasJson)
    {
        var enemySpawnDatas = new List<EnemySpawnData>();

        foreach (var enemyJson in enemySpawnDatasJson)
        {
            var enemySpawnData = new EnemySpawnData
            {
                EnemyModel = GetEnemyModel(enemyJson["enemyPrefabName"].ToString()),
                Count = enemyJson["count"]?.ToObject<int>() ?? 1
            };
            enemySpawnDatas.Add(enemySpawnData);
        }

        return enemySpawnDatas.ToArray();
    }

    public static Dungeon Parse(JObject dungeonJson)
    {
        var dungeon = new Dungeon();
        dungeon.StageData = dungeonJson;

        var waves = new List<Wave>();

        foreach (var waveJson in dungeonJson["waves"])
        {
            bool isBossWave = waveJson["bossPrefabName"] != null && waveJson["bossPrefabName"].Type != JTokenType.Null;

            Wave wave;
            if (isBossWave)
            {
                var bossWave = new BossWave
                {
                    enemySpawnDatas = ParseEnemySpawnDatas(waveJson["enemySpawnDatas"]),
                    BossModel = GetBossModel(waveJson["bossPrefabName"].ToString()),
                    BossPos = ParseVector3(waveJson["bossPosition"])
                };
                wave = bossWave;
            }
            else
            {
                wave = new Wave
                {
                    enemySpawnDatas = ParseEnemySpawnDatas(waveJson["enemySpawnDatas"])
                };
            }

            waves.Add(wave);
        }

        dungeon._waves = waves.ToArray();
        return dungeon;
    }

}
