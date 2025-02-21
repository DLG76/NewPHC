using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatEntity
{
    void Heal(float heal);
    void TakeDamage(float dmg);
    void Die();
    void ApplyForce(Vector2 vector, float power);
    T AddStatusEffect<T>() where T : StatusEffect;
    Transform GetTransform();
}
