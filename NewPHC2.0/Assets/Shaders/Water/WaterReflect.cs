using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterReflect : MonoBehaviour
{
    public float distanceFromParent = 1f;
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;
    public float childAlpha = 0.6f;

    private bool hasCloned = false;

    void Update()
    {
        if (!hasCloned)
        {
            GameObject childObject = new GameObject("ChildSprite");
            SpriteRenderer childRenderer = childObject.AddComponent<SpriteRenderer>();

            childRenderer.sprite = GetComponent<SpriteRenderer>().sprite;

            childObject.transform.SetParent(transform);
            childObject.transform.localPosition = new Vector3(0f, distanceFromParent, 0f);
            childObject.transform.localScale = new Vector3(1f, -1f, 1f);

            Color childColor = childRenderer.color;
            childColor.a = childAlpha;
            childRenderer.color = childColor;

            childRenderer.sortingLayerName = sortingLayerName;
            childRenderer.sortingOrder = orderInLayer;

            Animator parentAnimator = GetComponent<Animator>();
            if (parentAnimator != null)
            {
                Animator childAnimator = childObject.AddComponent<Animator>();
                childAnimator.runtimeAnimatorController = parentAnimator.runtimeAnimatorController;
            }

            hasCloned = true;
        }
    }
}