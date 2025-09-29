using UnityEngine;
using TMPro;

public class NumberMultiplierDisplay : MonoBehaviour
{
    public TMP_Text numberText;
    public float currentNumber = 1f;
    public float intervalSeconds = 5f;
    public float multiplier = 1.25f;
    public bool useUnscaledTime = false;
    public float maxMultiplier = 4f;

    private float baseNumber = 1f;

    private float timer = 0f;
    public Canvas mainMenuCanvas;
    public bool autoDetectParentCanvas = true;
    private bool started = false;

    void Start()
    {
        if (mainMenuCanvas == null && autoDetectParentCanvas)
        {
            mainMenuCanvas = GetComponentInParent<Canvas>();
        }

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
            Debug.LogWarning("NumberMultiplierDisplay: numberText 未指派，WTFFFFFFFFFFFFFF");
        }
        UpdateText();

        if (mainMenuCanvas == null || !mainMenuCanvas.gameObject.activeSelf)
        {
            started = true;
            baseNumber = currentNumber;
        }
    }

    void Update()
    {
        if (!started)
        {
            if (mainMenuCanvas == null || !mainMenuCanvas.gameObject.activeSelf)
            {
                started = true;
                timer = 0f;
            }
            else
            {
                return;
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
            numberText.text = "x" + currentNumber.ToString("F2");
        }
    }
}
