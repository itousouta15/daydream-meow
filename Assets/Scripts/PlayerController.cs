using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float airMoveSpeed = 12f;
    private Rigidbody2D rb;
    private LevelGenerator levelGenerator;
    private float moveInput = 0f;
    public float groundAcceleration = 100f;
    public float airAcceleration = 50f;
    public float airAccelerationInput = 120f;
    public float airDeceleration = 40f;
    public float carriedAcceleration = 200f;
    public float carriedMoveSpeed = 10f;
    public float pushedUpThreshold = 1.5f;

    private bool isOnPlatform = false;
    public float platformControlGrace = 0.35f;
    private float lastOnPlatformTime = -999f;
    public float xPadding = 0.5f;
    public Canvas mainMenuCanvas;
    public float topBuffer = 0.5f;
    public int lives = 9;
    public TMP_Text livesText;
    [Header("Lives Display")]
    public bool useHeartDisplay = true;
    public string lifeSymbol = "❤";
    public string lifeSeparator = " x ";
    public TMPro.TMP_FontAsset preferredHeartFont;
    [Header("Heart Images")]
    public bool useHeartImages = false;
    public Image heartPrefab;
    public RectTransform heartParent;
    public int maxHeartInstances = 20;
    private System.Collections.Generic.List<Image> heartImages = new System.Collections.Generic.List<Image>();
    [Header("Audio Clips")]
    public AudioClip landingSfx;
    public AudioClip waterHitSfx;
    public AudioClip deathSfx;
    public float invulnTime = 1f;
    private float invulnTimer = 0f;
    public float flashFrequency = 8f;
    [Range(0f,1f)]
    public float flashMinAlpha = 0.25f;
    private SpriteRenderer[] spriteRenderers;
    private Sprite[] originalSprites;
    private Color[] originalColors;
    private bool wasInvulnerable = false;
    public Sprite injuredSprite;
    private bool isShowingInjured = false;
    public Sprite deadSprite;
    public float deathShowDuration = 1.0f;
    public bool hasRaincoat = false;
    public Sprite raincoatSprite;
    private bool returnedToMenu = false;
    private AudioSource sfxSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        levelGenerator = UnityEngine.Object.FindAnyObjectByType<LevelGenerator>();
        var s = GetComponent<AudioSource>();
        if (s == null)
        {
            s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
        }
        sfxSource = s;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (livesText == null)
        {
            GameObject go = GameObject.Find("LivesText");
            if (go != null)
                livesText = go.GetComponent<TMP_Text>();
        }

        if (livesText == null)
        {
            livesText = UnityEngine.Object.FindAnyObjectByType<TMP_Text>();
            if (livesText != null)
                Debug.Log("PlayerController: 自動找到 TMP_Text 並指派給 livesText (" + livesText.gameObject.name + ")");
        }

        if (livesText == null)
        {
            Debug.LogWarning("PlayerController: livesText 尚未指定，生命顯示無法更新。請在 Inspector 指派一個 TextMeshPro 文字元件或建立 GameObject 命名為 'LivesText'.");
        }

        if (useHeartDisplay && livesText != null && !string.IsNullOrEmpty(lifeSymbol))
        {
            var fontAsset = livesText.font;
            bool hasGlyph = false;
            if (fontAsset != null)
            {
                char checkChar = lifeSymbol[0];
                hasGlyph = fontAsset.HasCharacter(checkChar);
            }

            if (!hasGlyph)
            {
                char checkChar = lifeSymbol[0];
                if (preferredHeartFont != null && preferredHeartFont.HasCharacter(checkChar))
                {
                    livesText.font = preferredHeartFont;
                    useHeartDisplay = true;
                    Debug.Log("PlayerController: 使用 preferredHeartFont 並啟用 heart display -> " + preferredHeartFont.name);
                }
                else
                {
                    TMPro.TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>();
                    TMPro.TMP_FontAsset found = null;
                    if (fonts != null && fonts.Length > 0)
                    {
                        foreach (var f in fonts)
                        {
                            if (f == null) continue;
                            if (f.HasCharacter(checkChar))
                            {
                                found = f;
                                break;
                            }
                        }
                    }

                    if (found != null && livesText != null)
                    {
                        livesText.font = found;
                        useHeartDisplay = true;
                        Debug.Log("PlayerController: 已自動指派支援心形的 TMP_FontAsset -> " + found.name + "，heart display 已重新啟用。");
                    }
                    else
                    {
                        Debug.LogWarning($"PlayerController: 目前字型不含 lifeSymbol ('{lifeSymbol}')，且未找到可用字型，已自動切換回純數字顯示以避免顯示方塊。");
                        useHeartDisplay = false;
                    }
                }
            }
        }

        UpdateLivesText();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            originalColors = new Color[spriteRenderers.Length];
            originalSprites = new Sprite[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalColors[i] = spriteRenderers[i].color;
                originalSprites[i] = spriteRenderers[i].sprite;
            }

            if (raincoatSprite == null)
            {
                Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var sp in allSprites)
                {
                    if (sp == null) continue;
                    if (sp.name == "雨衣貓")
                    {
                        raincoatSprite = sp;
                        Debug.Log("PlayerController: auto-assigned raincoatSprite -> " + sp.name);
                        break;
                    }
                }
                if (raincoatSprite == null)
                {
                    Sprite res = Resources.Load<Sprite>("雨衣貓");
                    if (res != null)
                    {
                        raincoatSprite = res;
                        Debug.Log("PlayerController: 從 Resources 載入 raincoatSprite -> " + res.name);
                    }
                    else
                    {
                        Debug.LogWarning("PlayerController: 未找到名為 '雨衣貓' 的 Sprite，請在 Inspector 手動指定 raincoatSprite。\n建議把圖片放到 Assets/Resources/ 並命名為 '雨衣貓.png'，或直接在 Inspector 指定。 ");
                    }
                }
                if (deadSprite == null)
                {
                    Sprite[] allSprites2 = Resources.FindObjectsOfTypeAll<Sprite>();
                    foreach (var sp2 in allSprites2)
                    {
                        if (sp2 == null) continue;
                        if (sp2.name == "死掉")
                        {
                            deadSprite = sp2;
                            Debug.Log("PlayerController: auto-assigned deadSprite -> " + sp2.name);
                            break;
                        }
                    }
                    if (deadSprite == null)
                    {
                        Sprite resDead = Resources.Load<Sprite>("死掉");
                        if (resDead != null)
                        {
                            deadSprite = resDead;
                            Debug.Log("PlayerController: 從 Resources 載入 deadSprite -> " + resDead.name);
                        }
                        else
                        {
                            Debug.LogWarning("PlayerController: 未找到名為 '死掉' 的 Sprite，請在 Inspector 手動指定 deadSprite，或放到 Assets/Resources/ 並命名為 '死掉.png'.");
                        }
                    }
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;

        // 判斷是否接觸到平台（包含水）且有接觸點在玩家下方 -> 視為落地
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y < transform.position.y - 0.1f)
            {
                // 落地音（避免在水上播放落地音）
                if (collision.gameObject.GetComponent<WaterPlatform>() == null)
                {
                    if (landingSfx != null)
                        sfxSource?.PlayOneShot(landingSfx);
                }
                else
                {
                    // 進入水域，播放踩水音（若會扣血，OnCollisionStay 將會呼叫 LoseLife，由 invuln 機制避免重複扣血）
                    if (waterHitSfx != null)
                        sfxSource?.PlayOneShot(waterHitSfx);
                }
                break;
            }
        }
    }

    IEnumerator ShowDeathThenReturn()
    {
        // 等待不受 timeScale 影響的一段時間
        yield return new WaitForSecondsRealtime(deathShowDuration);

        // 確保恢復時間尺度（保險）然後回主選單
        Time.timeScale = 1f;
        ReturnToMainMenu();
    }

    void Update() 
    {
        moveInput = 0f;
        var keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                moveInput = -1f;
            else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                moveInput = 1f;
        }
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
        if (invulnTimer > 0f)
        {
            wasInvulnerable = true;
            if (spriteRenderers != null)
            {
                float elapsed = invulnTime - invulnTimer;
                float s = (Mathf.Sin(elapsed * flashFrequency * Mathf.PI * 2f) + 1f) / 2f; // 0..1
                float alpha = Mathf.Lerp(flashMinAlpha, 1f, s);
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    Color c = originalColors[i];
                    c.a = originalColors[i].a * alpha;
                    spriteRenderers[i].color = c;
                }
            }
        }
        else if (wasInvulnerable)
        {
            wasInvulnerable = false;
            if (spriteRenderers != null)
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    if (spriteRenderers[i] != null)
                        spriteRenderers[i].color = originalColors[i];
                }
            }
            // 若正在顯示受傷造型，還原原本的 sprites
            if (isShowingInjured && originalSprites != null && spriteRenderers != null)
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    if (spriteRenderers[i] != null)
                        spriteRenderers[i].sprite = originalSprites[i];
                }
                isShowingInjured = false;
            }
        }

        // 沒站在平台且y速度接近0時，緩慢回到y=0
    if (!isOnPlatform && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            Vector3 pos = transform.position;
            float returnSpeed = 2f; // 回復速度可自行調整
            pos.y = Mathf.MoveTowards(pos.y, 0f, returnSpeed * Time.deltaTime);
            transform.position = pos;
        }

        // 如果玩家被擠出畫面上方，顯示主選單 Canvas
        if (!returnedToMenu && Camera.main != null)
        {
            float cameraTopY = Camera.main.transform.position.y + Camera.main.orthographicSize;
            if (transform.position.y > cameraTopY + topBuffer)
            {
                // 被擠出上方視窗視為受傷，扣生命並傳回原點
                LoseLife(true);
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 我們統一水平方向的最大移動速度為 moveSpeed，無論是在地面、空中或被平台帶上
        float targetVx = moveInput * moveSpeed;

        // 加速度規則：在地面使用 groundAcceleration，空中有輸入使用 airAccelerationInput，空中無輸入使用 airDeceleration
        float accel;
        if (isOnPlatform)
        {
            accel = groundAcceleration;
        }
        else
        {
            if (Mathf.Abs(moveInput) > 0.01f)
                accel = airAccelerationInput; // 空中有輸入，快速響應
            else
                accel = airDeceleration; // 空中無輸入，平滑減速
        }

        float newVx = Mathf.MoveTowards(rb.linearVelocity.x, targetVx, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);

        // 限制玩家不跑出攝影機左右畫面外
        if (Camera.main != null)
        {
            float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            float minX = Camera.main.transform.position.x - halfWidth + xPadding;
            float maxX = Camera.main.transform.position.x + halfWidth - xPadding;

            Vector2 pos = rb.position;
            float clampedX = Mathf.Clamp(pos.x, minX, maxX);
            if (!Mathf.Approximately(clampedX, pos.x))
            {
                // 如果超出邊界，夾住位置並清除水平速度，避免卡住或抖動
                rb.position = new Vector2(clampedX, pos.y);
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
    }

    // 只要碰到平台就算站在平台上

    void OnCollisionStay2D(Collision2D collision)
    {
        // 如果碰到的是水平台（WaterPlatform 組件），先檢查是否有雨衣
        if (collision.gameObject.GetComponent<WaterPlatform>() != null)
        {
            if (hasRaincoat)
            {
                // 消耗雨衣，保護一次
                ConsumeRaincoat();
                return;
            }
            else
            {
                LoseLife();
                return;
            }
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            // 只要有一個碰撞點在玩家下方就算站在平台上
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.point.y < transform.position.y - 0.1f)
                {
                    isOnPlatform = true;
                    lastOnPlatformTime = Time.time;
                    return;
                }
            }
        }
        isOnPlatform = false;
    }

    // 顯示主選單 Canvas 並暫停遊戲，停止玩家控制
    // 現在改為重新載入當前場景以刷新遊戲狀態（Reload），
    // 重新載入後 MainMenuCreator 可在場景啟動時顯示主選單。
    void ReturnToMainMenu()
    {
        if (returnedToMenu) return;
        returnedToMenu = true;

        // 還原時間尺度，確保場景載入正常
        Time.timeScale = 1f;

        // 重新載入當前場景以刷新所有狀態
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 更新生命顯示（若有指定 TMP）
    public void UpdateLivesText()
    {
        if (useHeartImages)
        {
            // Heart images 路徑：檢查 heartParent
            if (heartParent == null)
            {
                // 嘗試找到場景中的 Canvas 並建立一個 container
                var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject container = new GameObject("HeartsContainer");
                    container.transform.SetParent(canvas.transform, false);
                    var rt = container.AddComponent<RectTransform>();
                    var layout = container.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                    layout.spacing = 4f;
                    layout.childForceExpandHeight = false;
                    layout.childForceExpandWidth = false;
                    heartParent = rt;
                    Debug.Log("PlayerController: heartParent 自動建立 HeartsContainer under Canvas。");
                }
                else
                {
                    Debug.LogWarning("PlayerController: useHeartImages 為 true，但 heartParent 與 Canvas 未設定，沒救了);
                    useHeartImages = false;
                }
            }

            if (useHeartImages && heartParent != null)
            {
                if (livesText != null) livesText.gameObject.SetActive(false);

                bool usePrefab = heartPrefab != null;

                // 建立或重用心形 Image 到至少 min(lives, maxHeartInstances)
                int target = Mathf.Min(lives, maxHeartInstances);
                for (int i = 0; i < target; i++)
                {
                    if (i < heartImages.Count)
                    {
                        heartImages[i].gameObject.SetActive(true);
                        continue;
                    }

                    Image img;
                    if (usePrefab)
                    {
                        img = GameObject.Instantiate(heartPrefab, heartParent);
                    }
                    else
                    {
                        // fallback: 建立一個簡單的紅色方形 Image
                        GameObject go = new GameObject("HeartImage");
                        go.transform.SetParent(heartParent, false);
                        img = go.AddComponent<Image>();
                        img.color = Color.red;
                        var r = go.GetComponent<RectTransform>();
                        r.sizeDelta = new Vector2(24, 24);
                    }

                    img.gameObject.SetActive(true);
                    heartImages.Add(img);
                }

                // 隱藏多餘的 heart images
                for (int i = target; i < heartImages.Count; i++)
                {
                    heartImages[i].gameObject.SetActive(false);
                }

                Debug.Log($"PlayerController: 使用 Image 心形顯示，顯示 {Mathf.Min(lives, maxHeartInstances)} / {lives} (max {maxHeartInstances}) hearts.");
            }
        }
        else
        {
            // 使用文字顯示（舊方式）
            if (livesText != null) livesText.gameObject.SetActive(true);
            if (livesText != null)
            {
                if (useHeartDisplay)
                    livesText.text = lifeSymbol + lifeSeparator + lives.ToString();
                else
                    livesText.text = lives.ToString();
            }
        }
    }

    // 扣一條命（有無敵判定），生命歸零時回主選單
    public void LoseLife(bool teleportToOrigin = false)
    {
        // 若在無敵時間內，忽略
        if (invulnTimer > 0f) return;

        // 開始無敵計時
        invulnTimer = invulnTime;

        // 扣血並更新顯示
        lives = Mathf.Max(0, lives - 1);
        UpdateLivesText();

        // 若生命歸零，回主選單（透過 ReturnToMainMenu 處理）
        if (lives <= 0)
        {
            // 確保時間尺度是正常值
            Time.timeScale = 1f;

            // 顯示死亡造型（若有設定），停用玩家控制與物理模擬，然後短暫顯示後回主選單
            if (deadSprite != null && spriteRenderers != null)
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    if (spriteRenderers[i] != null)
                        spriteRenderers[i].sprite = deadSprite;
                }
            }

            // 停用玩家控制腳本以及模擬（避免在等待期間發生移動或被推）
            var pc = GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = false;
            if (rb != null)
                rb.simulated = false;

            // 使用 realtime 等待一段時間，然後返回主選單（不受 timeScale 影響）
            StartCoroutine(ShowDeathThenReturn());
        }
        else
        {
            // 還有生命：根據傳入參數決定是否把玩家移回原點
            // 受傷音效（非致命）——播放更大的音量以突顯受傷
            if (deathSfx != null && sfxSource != null)
            {
                float louder = 5f; // 音量放大（可在程式或 Inspector 調整）
                sfxSource.PlayOneShot(deathSfx, louder);
            }
            if (teleportToOrigin)
            {
                Vector3 newPos = transform.position;
                newPos.x = 0f;
                newPos.y = 0f;
                transform.position = newPos;
            }

            // 清除物理速度（避免被持續推動）
            rb.linearVelocity = Vector2.zero;

            // 清除狀態以重置碰撞判定與輸入
            isOnPlatform = false;
            moveInput = 0f;

            // 顯示受傷造型（若有設定）並標記，會在無敵結束時還原
            if (injuredSprite != null && spriteRenderers != null)
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    if (spriteRenderers[i] != null)
                        spriteRenderers[i].sprite = injuredSprite;
                }
                isShowingInjured = true;
            }
            // 可在這裡重置其他狀態或通知 LevelGenerator（若需要）
        }
    }

    // 供外部呼叫以重置 "已回到主選單" 的旗標，方便重新開始遊戲
    public void ResetReturnedFlag()
    {
        returnedToMenu = false;
    }

    // 供外部查詢是否正被平台帶上（簡單判斷：站在平台且場景滾動速度大於 0）
    public bool IsBeingCarried
    {
        get { return isOnPlatform && levelGenerator != null && levelGenerator.scrollSpeed > 0f; }
    }

    // 當與 trigger 互動時（例如撿到雨衣）
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        // 優先檢查 RaincoatPickup 組件（LevelGenerator 會自動加入）
        var pickup = other.GetComponent<RaincoatPickup>();
        if (pickup == null && other.attachedRigidbody != null)
            pickup = other.attachedRigidbody.GetComponent<RaincoatPickup>();
        if (pickup == null)
            pickup = other.GetComponentInParent<RaincoatPickup>();

        if (pickup != null)
        {
            EquipRaincoat();
            Destroy(pickup.gameObject);
            return;
        }

        // 否則退回到舊的 tag 判定（保留相容性）
        try
        {
            if (other.CompareTag("Raincoat"))
            {
                EquipRaincoat();
                Destroy(other.gameObject);
            }
        }
        catch (System.Exception)
        {
            // 忽略 Tag 例外
        }
    }

    public void EquipRaincoat()
    {
        hasRaincoat = true;
        // 若指定了 raincoatSprite，將所有子 SpriteRenderer 換成雨衣造型
        if (raincoatSprite != null && spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    spriteRenderers[i].sprite = raincoatSprite;
            }
        }
    }

    public void ConsumeRaincoat()
    {
        hasRaincoat = false;
        // 還原原本造型
        if (originalSprites != null && spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    spriteRenderers[i].sprite = originalSprites[i];
            }
        }
        // 給予短暫無敵（避免立即再受傷）
        invulnTimer = invulnTime;
    }
}
