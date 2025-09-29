using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldToStart : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float holdTime = 2f;
    public Canvas menuCanvas;
    public string playerTag = "Player";
    public LevelGenerator levelGen;
    public Image fillImage;

    private bool pressing = false;
    private float timer = 0f;

    void Reset()
    {
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
        pressing = false;
        timer = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    void OnHoldComplete()
    {
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);

        Time.timeScale = 1f;

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
            levelGen.ResumeAfterDelay(0f);
        }
    }
}
