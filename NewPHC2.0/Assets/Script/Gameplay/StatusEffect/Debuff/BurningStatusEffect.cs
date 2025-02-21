using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;
using UnityEngine.VFX;

public class BurningStatusEffect : StatusEffect
{
    public override bool CanStack => false;
    private Transform burningEntity;

    protected override void Awake()
    {
        base.Awake();

        AudioManager.Instance?.PlaySound("Burning", true, GetHashCode());
        burningEntity = Instantiate(Resources.Load<Transform>("Effect/BurningEffectEntity"), Vector3.zero, Quaternion.identity);
        onTick += (isLastTick) =>
        {
            if (isLastTick)
            {
                AudioManager.Instance?.StopSound("Burning", GetHashCode());
                Destroy(burningEntity.gameObject);
            }
        };
    }

    protected override void CreateEffect()
    {

    }

    protected override void Tick()
    {
        if (character != null)
            character.TakeDamage(point);
    }

    protected override void Update()
    {
        if (burningEntity != null)
            burningEntity.position = transform.position;

        base.Update();
    }

    private void OnDestroy()
    {
        if (burningEntity != null)
            Destroy(burningEntity.gameObject);
    }
}
