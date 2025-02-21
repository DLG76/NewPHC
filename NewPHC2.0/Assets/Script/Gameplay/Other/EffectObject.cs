using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectObject : MonoBehaviour
{
    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
