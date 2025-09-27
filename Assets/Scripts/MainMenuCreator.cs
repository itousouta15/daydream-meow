using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// 這個腳本會在執行時動態建立一個簡單的主選單 Canvas（預設為 inactive）
// 使用方式：將此腳本掛到場景中任意 GameObject 上，啟動遊戲時會自動建立 Canvas
// 並嘗試把它指定給場景中擁有 PlayerController 的物件（如果有且 autoAssignToPlayer=true）。

public class MainMenuCreator : MonoBehaviour
{
    [Tooltip("若未指定，會在啟動時自動建立一個 Canvas。")]
    public Canvas mainMenuCanvas;

    [Tooltip("是否自動把建立的 Canvas 指派給場景中的 PlayerController (會取得 tag 為 'Player' 的物件)。")]
    public bool autoAssignToPlayer = true;

    [Tooltip("玩家物件的 Tag (若 autoAssignToPlayer=true 時使用)")]
    public string playerTag = "Player";
    [Tooltip("遊戲開始時是否顯示主選單（須按 Enter 或按鈕開始）")]
    public bool showMenuOnStart = true;

    void Awake()
    {
        if (mainMenuCanvas == null)
        {
            mainMenuCanvas = CreateMainMenuCanvas();
            mainMenuCanvas.gameObject.SetActive(false);
        }

        if (autoAssignToPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                var pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.mainMenuCanvas = mainMenuCanvas;
                }
            }
        }

        // 如果要在開始時顯示主選單，啟用 Canvas 並暫停遊戲
        if (showMenuOnStart)
        {
            mainMenuCanvas.gameObject.SetActive(true);
            Time.timeScale = 0f;

            // 如果有玩家，停用玩家控制與模擬
            if (autoAssignToPlayer)
            {
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null)
                {
                    var pc = player.GetComponent<PlayerController>();
                    if (pc != null)
                        pc.enabled = false;
                    var rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.simulated = false;
                }
            }
        }
    }

    Canvas CreateMainMenuCanvas()
    {
        // Canvas
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 置於前景
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 背景 Panel (半透明遮罩)
        GameObject panelGO = new GameObject("Background");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // 主選單 Panel
        GameObject menuGO = new GameObject("MenuPanel");
        menuGO.transform.SetParent(canvasGO.transform, false);
        Image menuImage = menuGO.AddComponent<Image>();
        menuImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        RectTransform menuRT = menuGO.GetComponent<RectTransform>();
        menuRT.sizeDelta = new Vector2(400, 200);
        menuRT.anchorMin = new Vector2(0.5f, 0.5f);
        menuRT.anchorMax = new Vector2(0.5f, 0.5f);
        menuRT.anchoredPosition = Vector2.zero;

        // 標題文字
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(menuGO.transform, false);
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "Main Menu";
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 28;
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -10);
        titleRT.sizeDelta = new Vector2(0, 40);

        // 按鈕：回到遊戲（關閉主選單）
        GameObject btnResume = CreateButton("ResumeButton", "返回遊戲", new Vector2(0, -30));
        btnResume.transform.SetParent(menuGO.transform, false);
        btnResume.GetComponent<Button>().onClick.AddListener(() => {
            // 隱藏 Canvas 並恢復時間
            canvas.gameObject.SetActive(false);
            Time.timeScale = 1f;
            // 如果 player 被停用，重新啟用
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                var pc = player.GetComponent<PlayerController>();
                if (pc != null)
                    pc.enabled = true;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.simulated = true;
            }
        });

        // 按鈕：返回主選單場景（如果有實作）
        GameObject btnQuit = CreateButton("QuitButton", "離開到主選單", new Vector2(0, -90));
        btnQuit.transform.SetParent(menuGO.transform, false);
        btnQuit.GetComponent<Button>().onClick.AddListener(() => {
            Time.timeScale = 1f;
            // 如果你有 MainMenu 場景，請把名稱改成對應的場景名稱
            // SceneManager.LoadScene("MainMenu");
            Debug.Log("Quit to main menu pressed - implement scene load if desired.");
        });

        canvasGO.SetActive(false);
        return canvas;
    }

    void Update()
    {
        // 按 Enter 開始遊戲（當 Canvas 顯示時）
        if (showMenuOnStart && mainMenuCanvas != null && mainMenuCanvas.gameObject.activeSelf)
        {
            if (Keyboard.current != null && (Keyboard.current.enterKey.isPressed || Keyboard.current.numpadEnterKey.isPressed))
            {
                StartGame();
            }
        }
    }

    void StartGame()
    {
        if (mainMenuCanvas != null)
            mainMenuCanvas.gameObject.SetActive(false);

        Time.timeScale = 1f;

        // 啟用玩家控制與模擬
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
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
    }

    GameObject CreateButton(string name, string label, Vector2 anchoredPos)
    {
        GameObject btnGO = new GameObject(name);
        Button btn = btnGO.AddComponent<Button>();
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.9f);
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 40);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        Text txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.color = Color.black;
        RectTransform txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        return btnGO;
    }
}
