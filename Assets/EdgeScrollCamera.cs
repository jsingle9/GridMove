using UnityEngine;

public class EdgeScrollCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Edge Trigger (Viewport 0-1)")]
    [Range(0f, 0.49f)] public float edgeThreshold = 0.18f;

    [Header("Follow Through")]
    [Range(0f, 0.5f)] public float centerDeadZone = 0.04f; // how close to center before we stop
    public float maxScrollSpeed = 12f;                     // cap speed
    public float smoothTime = 0.12f;                       // damp time (smaller = snappier)

    private Camera cam;
    private Vector3 smoothVelocity;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);

        bool triggerX = viewportPos.x < edgeThreshold || viewportPos.x > 1f - edgeThreshold;
        bool triggerY = viewportPos.y < edgeThreshold || viewportPos.y > 1f - edgeThreshold;

        // Once triggered, continue moving camera toward target-centered position.
        // This is the key change that creates the "follow-through" feeling.
        Vector3 desiredPos = transform.position;
        desiredPos.x = target.position.x;
        desiredPos.y = target.position.y;
        // keep current camera z
        desiredPos.z = transform.position.z;

        Vector3 toDesired = desiredPos - transform.position;
        bool nearCenterX = Mathf.Abs(viewportPos.x - 0.5f) <= centerDeadZone;
        bool nearCenterY = Mathf.Abs(viewportPos.y - 0.5f) <= centerDeadZone;

        // Only move an axis if:
        // 1) it was edge-triggered, OR
        // 2) we are still not centered enough on that axis (follow-through continuation).
        bool moveX = triggerX || !nearCenterX;
        bool moveY = triggerY || !nearCenterY;

        // Axis-separated smoothing so we don't drift on axes that are already centered.
        Vector3 current = transform.position;
        float targetX = moveX ? desiredPos.x : current.x;
        float targetY = moveY ? desiredPos.y : current.y;

        float newX = Mathf.SmoothDamp(current.x, targetX, ref smoothVelocity.x, smoothTime, maxScrollSpeed, Time.deltaTime);
        float newY = Mathf.SmoothDamp(current.y, targetY, ref smoothVelocity.y, smoothTime, maxScrollSpeed, Time.deltaTime);

        transform.position = new Vector3(newX, newY, current.z);
    }
}
