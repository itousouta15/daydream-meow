using UnityEngine;
using TMPro;

public class meowcoin : MonoBehaviour
{
    public double total = 0.0;

    public NumberMultiplierDisplay multiplierDisplay;

    public bool autoFindDisplay = true;
    public TMP_Text totalText;
    public bool displayAsInteger = true;

    void Start()
    {
        if (multiplierDisplay == null && autoFindDisplay)
        {
            multiplierDisplay = Object.FindAnyObjectByType<NumberMultiplierDisplay>();
            if (multiplierDisplay == null)
            {
                Debug.LogWarning("meowcoin: 找不到 NumberMultiplierDisplay，請在 Inspector 指定或確認場景中有此元件。");
            }
        }

        if (totalText == null && autoFindDisplay)
        {
            var go = GameObject.Find("TotalText");
            if (go != null)
                totalText = go.GetComponent<TMP_Text>();

            if (totalText == null)
            {
                totalText = Object.FindAnyObjectByType<TMP_Text>();
                if (totalText != null)
                    Debug.Log("meowcoin: auto-assigned TMP_Text -> " + totalText.gameObject.name);
            }
        }
    }

    void Update()
    {
        if (multiplierDisplay != null)
        {
            total += multiplierDisplay.currentNumber * 2.0;
            if (totalText != null)
            {
                if (displayAsInteger)
                    totalText.text = ((int)total).ToString();
                else
                    totalText.text = total.ToString("F2");
            }
        }
    }
}
