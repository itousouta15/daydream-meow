using UnityEngine;
using TMPro;

// 取得 NumberMultiplierDisplay 的 currentNumber 並乘以 100
public class meowcoin : MonoBehaviour
{
    // 儲存結果
    public double total = 0.0;

    // 參考到場景中的 NumberMultiplierDisplay（如果你之前叫 TimedMultiplierDisplay，實際類名為 NumberMultiplierDisplay）
    public NumberMultiplierDisplay multiplierDisplay;

    // 若未在 Inspector 指定，是否自動尋找場景中的元件
    public bool autoFindDisplay = true;
    // TextMeshPro 顯示元件（選填）
    public TMP_Text totalText;
    // 顯示格式，若為 true 顯示整數（no decimals），否則顯示兩位小數
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

        // 嘗試自動找到 TMP 顯示元件（如果尚未指定）
        if (totalText == null && autoFindDisplay)
        {
            // 嘗試尋找名為 TotalText 的物件
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
            // 取得當前倍數並乘以 100
            total += multiplierDisplay.currentNumber * 2.0;
            // 更新 TMP 顯示
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
