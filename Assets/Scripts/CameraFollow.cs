using UnityEngine;

/// <summary>
/// 簡單且安全的相機跟隨 (Orthographic)
/// 使用方法：把此腳本掛到 Main Camera，將 target 指向玩家 Transform。
/// 功能：
/// - 只在玩家超出 deadZone（Viewport）時移動相機
/// - 平滑移動 (SmoothDamp)
/// - 不會改變玩家的物理或速度（只移動相機）
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Tooltip("要跟隨的目標 (Player)")]
    public Transform target;

    [Range(0f, 1f), Tooltip("希望玩家在畫面上的垂直位置（0=底，0.5=中，1=頂）")]
    public float desiredViewportY = 0.5f;

    [Tooltip("只有當玩家的 viewport y 超出 deadZone 時相機才會調整")]
    public float deadZone = 0.05f;

    [Tooltip("平滑時間 (較小較快)" )]
    public float smoothTime = 0.18f;

    [Tooltip("相機移動最大速度（世界單位/秒），設為 0 表示不限制")]
    public float maxSpeed = 20f;

    [Tooltip("垂直範圍限制（可選），設定為 large value 表示不限制")]
    public float minY = -9999f;
    public float maxY = 9999f;
    [Tooltip("當玩家被平台帶上時暫時不跟隨 (需 PlayerController 支援 IsBeingCarried)")]
    public bool ignoreWhenCarried = true;

    Camera cam;
    Vector3 velocity = Vector3.zero;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFollow 必須掛在有 Camera 的 GameObject 上。");
            enabled = false;
            return;
        }
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // 如果目標有 PlayerController，且正在被帶上，且選項打開，則不跟隨
        if (ignoreWhenCarried)
        {
            var pc = target.GetComponent<PlayerController>();
            if (pc != null && pc.IsBeingCarried) return;
        }

        // 取得目標在 viewport 的 y
        Vector3 vp = cam.WorldToViewportPoint(target.position);
        float deltaVP = vp.y - desiredViewportY; // 若為正代表目標在希望位置上方

        // 只有當超出 deadZone 時才調整相機
        if (Mathf.Abs(deltaVP) <= deadZone) return;

        // orthographic 世界高度 = camera.orthographicSize * 2
        float worldDeltaY = deltaVP * cam.orthographicSize * 2f;

        Vector3 current = transform.position;
        float targetY = current.y + worldDeltaY;

        // 套用上下限制
        targetY = Mathf.Clamp(targetY, minY, maxY);

        Vector3 desired = new Vector3(current.x, targetY, current.z);

        // 平滑移動（使用 SmoothDamp 保持穩定）
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
