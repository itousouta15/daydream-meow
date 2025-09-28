
using System.Collections.Generic;
using UnityEngine;

// 這個類別會自動產生平台並讓場景向上滾動，適合無限上升類型遊戲

public class LevelGenerator : MonoBehaviour
{
    // 平台的預製物 (Prefab)
    public GameObject platformPrefab;

    // 每個平台之間的垂直距離
    public float platformHeight = 2f;
    // 額外要增加的垂直間距（直接加到 platformHeight，預設 +2）
    public float extraSpacing = 2f;
    // 一開始要生成的平台數量
    public int initialPlatforms = 5;
    // 平台生成時 X 軸的隨機範圍
    public float xRange = 6f;
    // 場景向上滾動的速度
    public float scrollSpeed = 2f;
    // 每隔多少秒加速一次（單位秒）
    public float speedIncreaseInterval = 5f;
    // 每次加速的倍數（例如 1.5 表示變為 1.5 倍）
    public float speedMultiplier = 1.15f;
    // 是否在計算自動加速時使用 unscaled time（當 Time.timeScale = 0 時仍會累計）
    public bool useUnscaledTimeForSpeed = false;
    // 最大速度倍數 (相對於初始 scrollSpeed)，預設 3 倍
    public float maxMultiplier = 4f;
    // 計時器
    private float speedTimer = 0f;
    // 儲存初始速度以便做上限
    private float baseScrollSpeed;

    // 管理所有平台的 List
    private List<GameObject> platforms = new List<GameObject>();
    // 水平台機率（0..1）
    [Range(0f, 1f)]
    public float waterProbability = 0.3f;
    // 可指定水池與一般平台的 Sprite（如果平台使用 SpriteRenderer）
    public Sprite waterSprite;
    public Sprite defaultSprite;
    // 可指定整個水平台的 prefab，若指定則會用此 prefab 替換原平台（較方便的產量規則）
    public GameObject waterPlatformPrefab;
    // Optional: 雨衣拾取物的 prefab（可以在 Inspector 指定），若未指定可用 raincoatPickupSprite 自動建立
    public GameObject raincoatPrefab;
    public Sprite raincoatPickupSprite;
    // Spawn safety: 嘗試次數與避免重疊的半徑
    public int raincoatSpawnAttempts = 6;
    public float raincoatClearRadius = 0.5f;
    // 週期性生成雨衣（例如大約每 20 秒生成一個）
    public float raincoatSpawnInterval = 20f;
    private float raincoatSpawnTimer = 0f;
    // 是否使用 unscaled time 計時（當遊戲被暫停或 timeScale 改變時仍能生成）
    public bool useUnscaledTimeForRaincoat = false;
    // 每次重用平台時生成雨衣的機率（0..1）
    [Range(0f, 1f)]
    public float raincoatSpawnChance = 0.05f;
    // 相對於平台生成位置的偏移（預設在平台上方一點）
    public Vector2 raincoatSpawnOffset = new Vector2(0f, 0.6f);
    // overlay 選項：是否將 overlay 縮放以匹配平台的 BoxCollider2D
    public bool overlayMatchCollider = true;
    // overlay 的 sortingOrder 相對於平台 sprite 的偏移
    public int overlaySortingOrderOffset = 1;
    // overlay 固定世界尺寸選項（若啟用，會忽略 collider 並以此尺寸顯示 overlay，單位為 world units）
    public bool useOverlayFixedSize = false;
    // 預設 overlay 固定尺寸：寬 0.5、長 2
    public Vector2 overlayFixedSize = new Vector2(0.5f, 2f);

    // 遊戲開始時呼叫，初始化平台
    void Start()
    {
        // 如果沒有在 Inspector 指定 defaultSprite，嘗試從 platformPrefab 取得作為預設
        if (defaultSprite == null && platformPrefab != null)
        {
            var prefabSr = platformPrefab.GetComponent<SpriteRenderer>();
            if (prefabSr == null)
                prefabSr = platformPrefab.GetComponentInChildren<SpriteRenderer>();

            if (prefabSr != null)
            {
                defaultSprite = prefabSr.sprite;
                Debug.Log("LevelGenerator: defaultSprite 自動設定為 platformPrefab 的 sprite (" + (defaultSprite != null ? defaultSprite.name : "null") + ")");
            }
            else
            {
                Debug.LogWarning("LevelGenerator: 無法從 platformPrefab 取得 SpriteRenderer，defaultSprite 未設定。請在 Inspector 指定 defaultSprite。");
            }
        }

        baseScrollSpeed = scrollSpeed;
        float y = transform.position.y;
        for (int i = 0; i < initialPlatforms; i++)
        {
            GameObject plat = SpawnPlatform(y);
            platforms.Add(plat);
            y += GetSpacing();
        }
    }

    // 可用來暫停/恢復場景滾動與生成（例如主選單顯示時暫停）
    public bool paused = false;

    public void ResumeAfterDelay(float delaySeconds)
    {
        StopAllCoroutines();
        StartCoroutine(ResumeCoroutine(delaySeconds));
    }

    System.Collections.IEnumerator ResumeCoroutine(float delaySeconds)
    {
        // 使用 realtime 等待以避免受 timeScale 影響
        yield return new WaitForSecondsRealtime(delaySeconds);
        paused = false;
    }

    void SpawnRaincoat(Vector3 position)
    {
        // 嘗試找一個不會與其他碰撞體重疊的位置
        Vector3 spawnPos = position;
        bool placed = false;
        for (int attempt = 0; attempt < raincoatSpawnAttempts; attempt++)
        {
            // 若為第一次嘗試就用傳入位置，否則稍微隨機偏移
            if (attempt > 0)
            {
                spawnPos = position + (Vector3)Random.insideUnitCircle * raincoatClearRadius * 1.5f;
            }

            if (IsPositionClear(spawnPos, raincoatClearRadius))
            {
                placed = true;
                break;
            }
        }

        if (!placed)
        {
            // 找不到安全位置，放棄這次生成，避免卡在障礙內
            return;
        }

        if (raincoatPrefab != null)
        {
            Instantiate(raincoatPrefab, spawnPos, Quaternion.identity, transform);
            return;
        }

        // 若沒有 prefab，動態產生一個簡單的拾取物：SpriteRenderer + CircleCollider2D(isTrigger)
        GameObject go = new GameObject("RaincoatPickup");
        go.transform.SetParent(transform, false);
        go.transform.position = spawnPos;
        var sr = go.AddComponent<SpriteRenderer>();
        if (raincoatPickupSprite != null)
            sr.sprite = raincoatPickupSprite;
        else
            sr.color = Color.yellow; // 若沒有圖，給一個可見顏色

        // 為拾取物加入 RaincoatPickup 腳本以便處理拾取行為（比直接依賴 tag 更穩健）
        go.AddComponent<RaincoatPickup>();

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = Mathf.Max(0.25f, raincoatClearRadius * 0.8f);
    }

    // 檢查 spawn 位置附近是否有其他 collider（平台、水或現有拾取物）
    bool IsPositionClear(Vector3 pos, float radius)
    {
        // 使用 OverlapCircleAll 檢查 2D 碰撞
        var hits = Physics2D.OverlapCircleAll((Vector2)pos, radius);
        if (hits == null || hits.Length == 0) return true;

        foreach (var h in hits)
        {
            if (h == null) continue;
            // 忽略觸發器（小型 UI/trigger），但如果是實體平台 collider 或其他拾取物則視為阻擋
            if (h.isTrigger)
            {
                // 如果是其他 RaincoatPickup，也視為阻擋
                if (h.GetComponent<RaincoatPickup>() != null) return false;
                // 其它 trigger 可忽略
                continue;
            }

            // 如果碰撞到的是平台或水（透過存在 WaterPlatform 組件判斷），視為阻擋
            var wp = h.GetComponentInParent<WaterPlatform>();
            if (wp != null) return false;

            var plat = h.GetComponentInParent<SpriteRenderer>();
            if (plat != null) return false;
        }

        return true;
    }

    // 每幀呼叫，讓場景向上移動並重用平台
    void Update()
    {
        if (paused) return;
        // 場景（LevelGenerator 物件）向上移動
        transform.position += Vector3.up * scrollSpeed * Time.deltaTime;

        // 每隔 speedIncreaseInterval 秒，把 scrollSpeed 乘上 speedMultiplier
        if (speedIncreaseInterval > 0f)
        {
            float dt = useUnscaledTimeForSpeed ? Time.unscaledDeltaTime : Time.deltaTime;
            speedTimer += dt;
            if (speedTimer >= speedIncreaseInterval)
            {
                scrollSpeed *= speedMultiplier;
                // 套用上限
                float maxSpeed = baseScrollSpeed * maxMultiplier;
                if (scrollSpeed > maxSpeed) scrollSpeed = maxSpeed;
                speedTimer = 0f;
                Debug.Log("LevelGenerator: scrollSpeed increased to " + scrollSpeed + " (clamped to max " + maxSpeed + ")");
            }
        }

        // 取得攝影機下界 Y 座標
        float cameraBottomY = Camera.main.transform.position.y - Camera.main.orthographicSize;
        float cameraTopY = Camera.main.transform.position.y + Camera.main.orthographicSize;

        // 週期性生成雨衣（獨立於平台重用的隨機生成機制）
        if (raincoatSpawnInterval > 0f)
        {
            float dtR = useUnscaledTimeForRaincoat ? Time.unscaledDeltaTime : Time.deltaTime;
            raincoatSpawnTimer += dtR;
            if (raincoatSpawnTimer >= raincoatSpawnInterval)
            {
                // 在畫面下方附近產生一個雨衣，X 隨機
                float spawnX = Random.Range(-xRange, xRange);
                float spawnY = cameraBottomY + Mathf.Abs(raincoatSpawnOffset.y);
                SpawnRaincoat(new Vector3(spawnX, spawnY, 0f));
                raincoatSpawnTimer = 0f;
            }
        }

        // 使用索引迴圈以避免在列舉期間修改集合導致的 InvalidOperationException
        for (int i = 0; i < platforms.Count; i++)
        {
            GameObject plat = platforms[i];
            if (plat == null) continue;

            // 如果平台超出畫面上方一定距離，則移到相機底下並隨機 X
            if (plat.transform.position.y > cameraTopY + GetSpacing())
            {
                float newX = Random.Range(-xRange, xRange);
                float newY = cameraBottomY - GetSpacing() * 0.5f;
                plat.transform.position = new Vector3(newX, newY, 0);
                // 每次重用時重新決定是否成為水
                UpdatePlatformType(plat);

                // UpdatePlatformType 可能會以 prefab 替換 platforms 中的項目，
                // 因此在判定是否生成雨衣時重新讀取 platforms[i]
                GameObject currentPlat = (i >= 0 && i < platforms.Count) ? platforms[i] : plat;
                if (currentPlat == null) continue;

                // 若該平台不是水，則有機率在平台附近生成雨衣拾取物
                if (currentPlat.GetComponent<WaterPlatform>() == null && Random.value < raincoatSpawnChance)
                {
                    Vector3 spawnPos = currentPlat.transform.position + (Vector3)raincoatSpawnOffset;
                    SpawnRaincoat(spawnPos);
                }
            }
        }
    }

    // 生成一個平台在指定的 Y 座標，X 座標隨機
    GameObject SpawnPlatform(float y)
    {
        float x = Random.Range(-xRange, xRange);
        Vector3 spawnPos = new Vector3(x, y, 0);
        GameObject obj = Instantiate(platformPrefab, spawnPos, Quaternion.identity, transform);
        // 決定是否為水平台
        UpdatePlatformType(obj);
        return obj;
    }

    // 取得實際使用的間距（base + extra）
    float GetSpacing()
    {
        return platformHeight + extraSpacing;
    }

    // 根據機率把平台標為水或一般平台
    void UpdatePlatformType(GameObject plat)
    {
        // 移除舊的 WaterPlatform 標記（若存在）
        var existing = plat.GetComponent<WaterPlatform>();
        if (existing != null)
            Destroy(existing);

        // 嘗試取得 SpriteRenderer 來變更或設定 overlay（若有）
        var sr = plat.GetComponent<SpriteRenderer>();

        bool makeWater = (Random.value < waterProbability);
        if (makeWater)
        {
            // 如果使用者有提供整個水平台的 prefab，直接用 prefab 替換現有 platform（比較方便的生成規則）
            if (waterPlatformPrefab != null)
            {
                int idx = platforms.IndexOf(plat);
                Vector3 pos = plat.transform.position;
                Quaternion rot = plat.transform.rotation;
                Transform parent = plat.transform.parent;

                // 嘗試移除舊平台的 overlay（避免複製出現重疊）
                Transform oldOverlay = plat.transform.Find("WaterOverlay");
                if (oldOverlay != null)
                    GameObject.Destroy(oldOverlay.gameObject);

                // 記下舊平台的 SpriteRenderer（若有），以便複製 sorting 資訊
                SpriteRenderer oldSr = plat.GetComponent<SpriteRenderer>();

                // Instantiate new water platform
                GameObject newPlat = Instantiate(waterPlatformPrefab, pos, rot, parent);
                // 保持原本平台名稱（較利於除錯）
                newPlat.name = plat.name;

                // 確保標記
                if (newPlat.GetComponent<WaterPlatform>() == null)
                    newPlat.AddComponent<WaterPlatform>();

                // 如果 prefab 有 SpriteRenderer，把 sorting 設定從舊平台複製過去
                SpriteRenderer newSr = newPlat.GetComponent<SpriteRenderer>();
                if (newSr != null && oldSr != null)
                {
                    newSr.sortingLayerID = oldSr.sortingLayerID;
                    newSr.sortingOrder = oldSr.sortingOrder;
                }

                // 停用舊平台的可見 SpriteRenderer 以避免在下一個畫格出現重疊
                if (oldSr != null)
                    oldSr.enabled = false;

                // 刪除舊的物件（會在下一個 frame 實際移除）
                Destroy(plat);

                // 將 platforms 列表中的參考替換為新物件
                if (idx >= 0)
                    platforms[idx] = newPlat;

                // 更新參考，接著可針對 newPlat 設定 overlay（如果需要）
                plat = newPlat;
                sr = plat.GetComponent<SpriteRenderer>();
            }
            else
            {
                // 沒有 prefab 時，保留原本的 overlay/顏色邏輯來表現水面
                plat.AddComponent<WaterPlatform>();
            }

            // 若有指定 waterSprite，使用 overlay 顯示水面（即使使用 waterPlatformPrefab，也能套 overlay）
            if (waterSprite != null)
            {
                Transform overlayT = plat.transform.Find("WaterOverlay");
                SpriteRenderer overlaySr = null;
                if (overlayT == null)
                {
                    GameObject overlayGO = new GameObject("WaterOverlay");
                    overlayGO.transform.SetParent(plat.transform, false);
                    overlaySr = overlayGO.AddComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        overlaySr.sortingLayerID = sr.sortingLayerID;
                        overlaySr.sortingOrder = sr.sortingOrder + overlaySortingOrderOffset;
                    }
                }
                else
                {
                    overlaySr = overlayT.GetComponent<SpriteRenderer>();
                    if (overlaySr == null) overlaySr = overlayT.gameObject.AddComponent<SpriteRenderer>();
                }

                overlaySr.sprite = waterSprite;
                overlaySr.color = Color.white;

                // 確保 overlay 的 sorting 與位置設定（如果有平台的 SpriteRenderer，使用其 sorting 並加上偏移）
                if (sr != null)
                {
                    overlaySr.sortingLayerID = sr.sortingLayerID;
                    overlaySr.sortingOrder = sr.sortingOrder + overlaySortingOrderOffset;
                }

                // 大小與位置：優先使用固定世界尺寸；否則若 overlayMatchCollider 為 true，則匹配 collider；最後使用預設 scale
                if (useOverlayFixedSize)
                {
                    if (overlaySr.sprite != null)
                    {
                        Vector2 spriteWorldSize = overlaySr.sprite.bounds.size;
                        Vector3 newScale = Vector3.one;
                        if (spriteWorldSize.x != 0f) newScale.x = overlayFixedSize.x / spriteWorldSize.x;
                        if (spriteWorldSize.y != 0f) newScale.y = overlayFixedSize.y / spriteWorldSize.y;
                        overlaySr.transform.localScale = newScale;
                    }
                    else
                    {
                        overlaySr.transform.localScale = new Vector3(overlayFixedSize.x, overlayFixedSize.y, 1f);
                    }
                    overlaySr.transform.localPosition = Vector3.zero;
                }
                else if (overlayMatchCollider)
                {
                    var box = plat.GetComponent<BoxCollider2D>();
                    if (box == null) box = plat.GetComponentInChildren<BoxCollider2D>();
                    if (box != null)
                    {
                        overlaySr.transform.localPosition = box.offset;
                        overlaySr.transform.localScale = new Vector3(box.size.x, box.size.y, 1f);
                    }
                    else
                    {
                        overlaySr.transform.localScale = Vector3.one;
                        overlaySr.transform.localPosition = Vector3.zero;
                    }
                }
                else
                {
                    overlaySr.transform.localScale = Vector3.one;
                    overlaySr.transform.localPosition = Vector3.zero;
                }
            }
            else
            {
                // 若沒有 waterSprite，就用簡單的藍色 tint（保持向下相容）
                if (sr != null)
                    sr.color = Color.blue;
            }
        }
        else
        {
            // 一般平台，還原顏色
            // 移除或停用 overlay（如果存在）
            Transform overlayT = plat.transform.Find("WaterOverlay");
            if (overlayT != null)
            {
                GameObject.Destroy(overlayT.gameObject);
            }

            if (sr != null)
            {
                if (defaultSprite != null)
                    sr.sprite = defaultSprite;
                sr.color = Color.white;
            }
        }
    }
}
