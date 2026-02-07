using UnityEngine;

public class EdgeScrollCamera : MonoBehaviour
{
    public Transform target;            // The box
    public float scrollSpeed = 5f;       // Camera movement speed
    public float edgeThreshold = 0.4f;   // Distance from screen edge (0–1)

    void LateUpdate()
    {
        if (target == null) return;

        Camera cam = Camera.main;

        // Convert target position to viewport space (0–1)
        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);

        Vector3 move = Vector3.zero;

        // Horizontal scrolling
        if (viewportPos.x < edgeThreshold)
            move.x = -1;
        else if (viewportPos.x > 1f - edgeThreshold)
            move.x = 1;

        // Vertical scrolling
        if (viewportPos.y < edgeThreshold)
            move.y = -1;
        else if (viewportPos.y > 1f - edgeThreshold)
            move.y = 1;

        // Move camera
        transform.position += move * scrollSpeed * Time.deltaTime;
    }
}
