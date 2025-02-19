using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    public float dragSpeed = 1;
    private Vector3 dragOriginWorld;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOriginWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            Drag();
        }
    }

    private void Drag()
    {
        Vector3 currentMouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 move = dragOriginWorld - currentMouseWorld;

        Camera.main.transform.position += move * dragSpeed;
    }
}
