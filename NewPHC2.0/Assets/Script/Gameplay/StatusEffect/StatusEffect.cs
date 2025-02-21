using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class StatusEffect : MonoBehaviour
{
    public abstract bool CanStack { get; }
    public UnityAction<bool> onTick;

    [SerializeField] private float tickTime;
    [SerializeField] private int maxTickCount;

    protected float tickCount = 0;
    protected float lastTickTime;
    protected float point;

    protected CharacterCombat character;

    protected bool startTick = false;

    protected virtual void Awake()
    {
        lastTickTime = Time.time;

        character = GetComponent<CharacterCombat>();
    }

    public virtual void Setup(float tickTime, int maxTickCount, float point)
    {
        this.tickTime = tickTime;
        this.maxTickCount = maxTickCount;
        this.point = point;

        startTick = true;
    }

    protected virtual void Update()
    {
        if (!startTick) return;

        if (tickCount >= maxTickCount)
            Destroy(this);

        if (Time.time - lastTickTime >= tickTime)
        {
            tickCount++;

            onTick?.Invoke(tickCount >= maxTickCount);

            lastTickTime = Time.time;

            Tick();
            CreateEffect();
        }
    }

    protected abstract void Tick();

    protected abstract void CreateEffect();

    private void OnDestroy()
    {
        onTick?.Invoke(true);
    }
}
