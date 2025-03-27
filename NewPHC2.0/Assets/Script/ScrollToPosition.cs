using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollToPosition : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float targetY;
    public float scrollSpeed = 5f; // Adjust speed for smooth scrolling

    public void ScrollToY()
    {
        StopAllCoroutines(); // Stop any ongoing scroll
        StartCoroutine(SmoothScroll(targetY));
    }

    IEnumerator SmoothScroll(float targetY)
    {
        while (Mathf.Abs(scrollRect.content.anchoredPosition.y - targetY) > 1f)
        {
            Vector2 newPosition = scrollRect.content.anchoredPosition;
            newPosition.y = Mathf.Lerp(newPosition.y, targetY, Time.deltaTime * scrollSpeed);
            scrollRect.content.anchoredPosition = newPosition;
            yield return null;
        }
    }
}