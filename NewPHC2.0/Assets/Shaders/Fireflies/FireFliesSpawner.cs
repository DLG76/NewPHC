using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFliesSpawner : MonoBehaviour
{
    public GameObject fireflyPrefab;
    public float spawnInterval = 2f;
    public float maxSpawnRadius = 5f;
    public int maxFirefliesPerSpawner = 5;

    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - spawnTime >= spawnInterval)
        {
            for (int i = 0; i < maxFirefliesPerSpawner; i++)
            {
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                float randomRadius = Random.Range(0f, maxSpawnRadius);
                Vector3 spawnPosition = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0f) * randomRadius;

                Instantiate(fireflyPrefab, transform.position + spawnPosition, Quaternion.identity,GameObject.Find("FireFliesSpawnerAll").transform);
            }
            spawnTime = Time.time;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);
    }
}




