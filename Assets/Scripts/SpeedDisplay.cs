using UnityEngine;
using UnityEngine.UI;

// 在畫面右上角顯示目前的速度倍數（例如 Speed x1.50）
public class SpeedDisplay : MonoBehaviour
{
    public LevelGenerator levelGenerator;
    private Text speedText;

    void Awake()
    {
        if (levelGenerator == null)
        {
            // 使用新的 API 以避免過時警告
            levelGenerator = UnityEngine.Object.FindAnyObjectByType<LevelGenerator>();
        }

        // 嘗試找到現有的 Canvas
        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("HUDCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // 建立文字顯示在右上角
        GameObject txtGO = new GameObject("SpeedText");
        txtGO.transform.SetParent(canvas.transform, false);
        speedText = txtGO.AddComponent<Text>();
        speedText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        speedText.fontSize = 18;
        speedText.alignment = TextAnchor.UpperRight;
        speedText.color = Color.white;

        RectTransform rt = txtGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-10, -10);
        rt.sizeDelta = new Vector2(200, 40);
    }

    void Update()
    {
        if (levelGenerator == null || speedText == null) return;

        float multiplier = 1f;
        if (levelGenerator.baseScrollSpeed > 0f)
            multiplier = levelGenerator.scrollSpeed / levelGenerator.baseScrollSpeed;

        speedText.text = $"Speed x{multiplier:F2}";
    }
}
