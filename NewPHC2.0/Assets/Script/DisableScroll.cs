using UnityEngine;
using UnityEngine.UI;

public class DisableScroll : MonoBehaviour
{
    public ScrollRect scrollRect; // Assign in Inspector
    

    public void DisableScrollRect()
    {
        scrollRect.enabled = false; // Completely disables scrolling
    }

    public void EnableScrollRect()
    {
        scrollRect.enabled = true; // Re-enable scrolling
    }
}