using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Range(0f, 1f)]
    public float desiredViewportY = 0.5f;

    public float deadZone = 0.05f;

    public float smoothTime = 0.18f;

    public float maxSpeed = 20f;

    public float minY = -9999f;
    public float maxY = 9999f;
    public bool ignoreWhenCarried = true;

    Camera cam;
    Vector3 velocity = Vector3.zero;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            enabled = false;
            return;
        }
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        if (ignoreWhenCarried)
        {
            var pc = target.GetComponent<PlayerController>();
            if (pc != null && pc.IsBeingCarried) return;
        }

        Vector3 vp = cam.WorldToViewportPoint(target.position);
        float deltaVP = vp.y - desiredViewportY;

        if (Mathf.Abs(deltaVP) <= deadZone) return;

        float worldDeltaY = deltaVP * cam.orthographicSize * 2f;

        Vector3 current = transform.position;
        float targetY = current.y + worldDeltaY;

        targetY = Mathf.Clamp(targetY, minY, maxY);

        Vector3 desired = new Vector3(current.x, targetY, current.z);

        if (maxSpeed > 0f)
        {
            transform.position = Vector3.SmoothDamp(current, desired, ref velocity, smoothTime, maxSpeed);
        }
        else
        {
            transform.position = Vector3.SmoothDamp(current, desired, ref velocity, smoothTime);
        }
    }
}
