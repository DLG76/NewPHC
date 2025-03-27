using UnityEngine;
using Cinemachine;

public class CameraDrag : MonoBehaviour
{
    public float dragSpeed = 1f;
    public CinemachineVirtualCamera virtualCamera;
    public Collider2D confinerCollider; // Assign PolygonCollider2D of Cinemachine Confiner

    private Vector3 dragOriginWorld;
    private Transform followTarget;

    private void Start()
    {
        if (virtualCamera != null)
        {
            followTarget = new GameObject("CameraFollowTarget").transform;
            followTarget.position = virtualCamera.transform.position;
            virtualCamera.Follow = followTarget;
        }
        else
        {
            Debug.LogError("CinemachineVirtualCamera is not assigned!");
        }

        if (confinerCollider == null)
        {
            Debug.LogError("Confiner Collider2D is not assigned!");
        }
    }

    private void Update()
    {
        if (virtualCamera == null || followTarget == null || confinerCollider == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            dragOriginWorld = GetMouseWorldPosition();
        }

        if (Input.GetMouseButton(1))
        {
            Drag();
        }
    }

    private void Drag()
    {
        Vector3 currentMouseWorld = GetMouseWorldPosition();
        Vector3 move = dragOriginWorld - currentMouseWorld;

        // คำนวณตำแหน่งใหม่ที่อยากให้เคลื่อนที่ไป
        Vector3 newPosition = followTarget.position + new Vector3(move.x, move.y, 0f) * dragSpeed;

        // ✅ ปรับตำแหน่งให้ยังอยู่ในขอบของ Confiner 2D
        followTarget.position = ClampPositionToConfiner(newPosition);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector3(mouseWorld.x, mouseWorld.y, 0f);
    }

    private Vector3 ClampPositionToConfiner(Vector3 position)
    {
        if (confinerCollider == null) return position;

        // ✅ ดึง Boundary (ขอบเขต) ของ Collider2D
        Bounds bounds = confinerCollider.bounds;

        // ✅ จำกัดตำแหน่ง X และ Y ให้อยู่ใน Bounds
        float clampedX = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        float clampedY = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);

        return new Vector3(clampedX, clampedY, position.z);
    }
}
