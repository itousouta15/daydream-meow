using UnityEngine;
using TMPro;

public class NumberMultiplierDisplay : MonoBehaviour
{
    public TMP_Text numberText;    // 連結 TMP UI 文字元件
    public float currentNumber = 1f;
    public float intervalSeconds = 5f;
    public float multiplier = 1.25f;
    // 若遊戲被暫停 (Time.timeScale = 0)，是否仍使用 unscaled time 讓數字繼續累進
    public bool useUnscaledTime = false;
    // 最大倍數上限（相對於起始 currentNumber），例如 3 表示最多為起始值的 3 倍
    public float maxMultiplier = 4f;

    // 紀錄啟動時的基底數值，用來計算最大值
    private float baseNumber = 1f;

    private float timer = 0f;
    // 如果主選單 Canvas 為這個 TMP 的父物件，可指定它以在 Canvas 隱藏後才開始運作
    public Canvas mainMenuCanvas;
    // 若未手動指定，是否自動搜尋父層的 Canvas
    public bool autoDetectParentCanvas = true;
    // 是否已開始運作（按 Enter 關閉主選單後會開始）
    private bool started = false;

    void Start()
    {
        // 嘗試自動取得父 Canvas（如果使用者沒在 Inspector 指定）
        if (mainMenuCanvas == null && autoDetectParentCanvas)
        {
            mainMenuCanvas = GetComponentInParent<Canvas>();
        }

        // 若 numberText 未指派，嘗試從同一 GameObject 或子物件取得
        if (numberText == null)
        {
            numberText = GetComponent<TMP_Text>();
            if (numberText == null)
            {
                numberText = GetComponentInChildren<TMP_Text>();
            }
        }

        if (numberText == null)
        {
            Debug.LogWarning("NumberMultiplierDisplay: numberText 未指派，將無法更新顯示。");
        }
        UpdateText();

        // 如果沒有指定主選單 Canvas，或 Canvas 目前已經隱藏，直接開始
        if (mainMenuCanvas == null || !mainMenuCanvas.gameObject.activeSelf)
        {
            started = true;
            // 記錄基底數值，之後以此為基準做上限
            baseNumber = currentNumber;
        }
    }

    void Update()
    {
        // 如果尚未開始，檢查主選單 Canvas 是否已隱藏（代表按下 Enter 開始遊戲）
        if (!started)
        {
            if (mainMenuCanvas == null || !mainMenuCanvas.gameObject.activeSelf)
            {
                started = true;
                timer = 0f; // 重置計時器
            }
            else
            {
                return; // 尚未開始，跳過計時
            }
        }

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timer += dt;
        if (timer >= intervalSeconds)
        {
            timer = 0f;
            MultiplyNumber();
            UpdateText();
        }
    }

    void MultiplyNumber()
    {
        currentNumber *= multiplier;
        // 限制最大值為 baseNumber * maxMultiplier
        float cap = baseNumber * Mathf.Max(1f, maxMultiplier);
        if (currentNumber > cap)
        {
            currentNumber = cap;
        }
    }

    void UpdateText()
    {
        if (numberText != null)
        {
            numberText.text = "x" + currentNumber.ToString("F2"); // 在數字前加上 x，顯示兩位小數
        }
    }
}
