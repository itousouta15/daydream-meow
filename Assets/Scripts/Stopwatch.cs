using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Stopwatch : MonoBehaviour
{
    public TMP_Text timeText;

    public bool startOnAwake = false;

    public bool useUnscaledTime = false;

    public float updateInterval = 0.05f;

    public bool autoFindDisplay = true;

    public bool showMilliseconds = true;

    private float elapsed = 0f;
    private bool running = false;
    private float displayTimer = 0f;

    public List<float> laps = new List<float>();

    public float ElapsedSeconds => elapsed;
    public bool IsRunning => running;

    void Start()
    {
        if (timeText == null && autoFindDisplay)
        {
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

    public void StartStopwatch()
    {
        running = true;
    }

    public void StopStopwatch()
    {
        running = false;
        UpdateDisplay();
    }

    public void Toggle()
    {
        if (running) StopStopwatch(); else StartStopwatch();
    }

    public void ResetStopwatch()
    {
        elapsed = 0f;
        laps.Clear();
        UpdateDisplay();
    }

    public void Lap()
    {
        laps.Add(elapsed);
    }

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
