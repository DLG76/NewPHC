using UnityEngine;
using Cinemachine;

public class CameraDrag : MonoBehaviour
{
    public float dragSpeed = 1f;
    public CinemachineVirtualCamera virtualCamera; // Reference to Cinemachine Virtual Camera
    private Vector3 dragOriginWorld;
    private Transform cameraTransform;

    private void Start()
    {
        if (virtualCamera != null)
        {
            cameraTransform = virtualCamera.transform; // Get the Cinemachine Virtual Camera transform
        }
        else
        {
            Debug.LogError("CinemachineVirtualCamera is not assigned!");
        }
    }

    private void Update()
    {
        if (virtualCamera == null) return;

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

        cameraTransform.position += move * dragSpeed;
    }
}
