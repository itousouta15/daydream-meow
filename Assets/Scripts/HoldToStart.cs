using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 按住按鈕一段時間後觸發開始行為（使用 realtime / unscaled time）
public class HoldToStart : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float holdTime = 2f; // required seconds to hold
    public Canvas menuCanvas; // the main menu canvas to hide
    public string playerTag = "Player";
    public LevelGenerator levelGen; // optional reference
    public Image fillImage; // optional visual fill (Image type, should be filled)

    private bool pressing = false;
    private float timer = 0f;

    void Reset()
    {
        // default for convenience when added in editor
        holdTime = 2f;
    }

    void Update()
    {
        if (!pressing) return;
        timer += Time.unscaledDeltaTime;
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(timer / holdTime);

        if (timer >= holdTime)
        {
            pressing = false;
            OnHoldComplete();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressing = true;
        timer = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressing = false;
        timer = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // cancel when pointer leaves the button
        pressing = false;
        timer = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    void OnHoldComplete()
    {
        // Hide menu and resume game
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);

        Time.timeScale = 1f;

        // enable player control and physics if player exists
        GameObject player = null;
        try { player = GameObject.FindGameObjectWithTag(playerTag); } catch { }
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = true;
                pc.ResetReturnedFlag();
            }
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.simulated = true;
        }

        if (levelGen == null)
            levelGen = Object.FindAnyObjectByType<LevelGenerator>();
        if (levelGen != null)
        {
            // resume immediately
            levelGen.ResumeAfterDelay(0f);
        }
    }
}
