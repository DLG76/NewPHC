using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFliesController : MonoBehaviour
{
    public float minLerpSpeed = 0.5f;
    public float maxLerpSpeed = 2f;
    public float minActionInterval = 2f;
    public float maxActionInterval = 5f;
    public float lifeTime = 10f;
    public float movementRadius = 4f;

    private UnityEngine.Rendering.Universal.Light2D fireflyLight;
    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private float lerpSpeed;
    private float nextActionTime;
    private float actionInterval;
    private float spawnTime;

    private void Start()
    {
        fireflyLight = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        fireflyLight.enabled = false;
        initialPosition = transform.position;
        targetPosition = GetRandomPositionInRadius(initialPosition, movementRadius);
        lerpSpeed = Random.Range(minLerpSpeed, maxLerpSpeed);
        actionInterval = Random.Range(minActionInterval, maxActionInterval);
        spawnTime = Time.time;
        nextActionTime = Time.time + Random.Range(0f, actionInterval);
    }

    private void Update()
    {
        if (Time.time - spawnTime >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time >= nextActionTime)
        {
            fireflyLight.enabled = !fireflyLight.enabled;
            if (fireflyLight.enabled)
            {
                targetPosition = GetRandomPositionInRadius(initialPosition, movementRadius);
            }
            nextActionTime = Time.time + actionInterval;
        }
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
    }

    private Vector3 GetRandomPositionInRadius(Vector3 center, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        return center + new Vector3(randomDirection.x, randomDirection.y, 0f) * radius;
    }
}