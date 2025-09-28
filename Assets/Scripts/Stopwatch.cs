using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 簡單的碼表 / 計時器元件
/// 功能：開始、停止、重置、圈次 (Lap)、切換、支援 unscaled time
/// 將此腳本掛到任一 GameObject，並在 Inspector 指定一個 TextMeshPro 元件來顯示時間。
/// </summary>
public class Stopwatch : MonoBehaviour
{
    [Tooltip("將要顯示時間的 TextMeshPro 元件，可在 Inspector 指派或使用 autoFindDisplay 自動找到場景中的第一個 TMP_Text")]
    public TMP_Text timeText;

    [Tooltip("啟動時自動開始")]
    public bool startOnAwake = false;

    [Tooltip("使用 unscaled time（當遊戲暫停時仍會繼續）")]
    public bool useUnscaledTime = false;

    [Tooltip("更新顯示的頻率（秒）")]
    public float updateInterval = 0.05f;

    [Tooltip("若未在 Inspector 指定 timeText，是否自動尋找場景中的 TMP_Text")]
    public bool autoFindDisplay = true;

    [Tooltip("是否在顯示上顯示毫秒（格式 hh:mm:ss.ff），否則顯示 hh:mm:ss")]
    public bool showMilliseconds = true;

    // 目前累計時間（秒）
    private float elapsed = 0f;
    // 是否正在計時
    private bool running = false;
    // 用於控制更新顯示頻率
    private float displayTimer = 0f;

    // 紀錄圈次（秒數）
    public List<float> laps = new List<float>();

    // 供外部讀取
    public float ElapsedSeconds => elapsed;
    public bool IsRunning => running;

    void Start()
    {
        if (timeText == null && autoFindDisplay)
        {
            // 先嘗試尋找名為 StopwatchText 的物件
            var go = GameObject.Find("StopwatchText");
            if (go != null)
                timeText = go.GetComponent<TMP_Text>();

            if (timeText == null)
            {
                timeText = Object.FindAnyObjectByType<TMP_Text>();
                if (timeText != null)
                    Debug.Log("Stopwatch: auto-assigned TMP_Text -> " + timeText.gameObject.name);
            }
        }

        if (startOnAwake)
            StartStopwatch();

        UpdateDisplay();
    }

    void Update()
    {
        if (!running) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        elapsed += dt;
        displayTimer += dt;
        if (displayTimer >= updateInterval)
        {
            displayTimer = 0f;
            UpdateDisplay();
        }
    }

    // 開始計時
    public void StartStopwatch()
    {
        running = true;
    }

    // 停止計時（暫停）
    public void StopStopwatch()
    {
        running = false;
        UpdateDisplay();
    }

    // 切換開始/停止
    public void Toggle()
    {
        if (running) StopStopwatch(); else StartStopwatch();
    }

    // 重置
    public void ResetStopwatch()
    {
        elapsed = 0f;
        laps.Clear();
        UpdateDisplay();
    }

    // 登記一個圈次（紀錄當前 elapsed）
    public void Lap()
    {
        laps.Add(elapsed);
    }

    // 更新顯示文字
    void UpdateDisplay()
    {
        if (timeText == null) return;
        timeText.text = FormatTime(elapsed);
    }

    string FormatTime(float t)
    {
        int hours = (int)(t / 3600f);
        int minutes = (int)((t % 3600f) / 60f);
        int seconds = (int)(t % 60f);
        if (showMilliseconds)
        {
            int centiseconds = (int)((t - Mathf.Floor(t)) * 100f);
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", hours, minutes, seconds, centiseconds);
        }
        else
        {
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }
}
