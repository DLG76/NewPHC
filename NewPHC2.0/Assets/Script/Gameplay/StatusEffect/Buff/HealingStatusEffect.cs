using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingStatusEffect : StatusEffect
{
    public override bool CanStack => true;

    protected override void CreateEffect()
    {
        if (character != null)

        Instantiate(Resources.Load<Transform>("Effect/HealingEffectEntity"), RandomPositionGenerator.GetRandomPositionInCollider(character.Collider), Quaternion.Euler(0, 0, Random.Range(0, 360)));
        AudioManager.Instance?.PlaySound("Healing", 0.25f, Random.Range(0.9f, 1.1f));
    }

    protected override void Tick()
    {
        if (character != null)
            character.Heal(point);
    }
}
