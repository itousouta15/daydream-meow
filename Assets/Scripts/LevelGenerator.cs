
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
    // 儲存初始速度以便計算倍數（在 Start 會被設定）
    [HideInInspector]
    public float baseScrollSpeed = 0f;
    // 每隔多少秒加速一次（單位秒）
    public float speedIncreaseInterval = 5f;
    // 每次加速的倍數（例如 1.5 表示變為 1.5 倍）
    public float speedMultiplier = 1.25f;
    // 計時器
    private float speedTimer = 0f;

    // 管理所有平台的 List
    private List<GameObject> platforms = new List<GameObject>();
    // 水平台機率（0..1）
    [Range(0f, 1f)]
    public float waterProbability = 0.3f;

    // 遊戲開始時呼叫，初始化平台
    void Start()
    {
        // 記住初始速度，用來計算倍數顯示
        baseScrollSpeed = scrollSpeed;
        float y = transform.position.y;
        for (int i = 0; i < initialPlatforms; i++)
        {
            GameObject plat = SpawnPlatform(y);
            platforms.Add(plat);
            y += GetSpacing();
        }
    }

    // 每幀呼叫，讓場景向上移動並重用平台
    void Update()
    {
        // 場景（LevelGenerator 物件）向上移動
        transform.position += Vector3.up * scrollSpeed * Time.deltaTime;

        // 每隔 speedIncreaseInterval 秒，把 scrollSpeed 乘上 speedMultiplier
        if (speedIncreaseInterval > 0f)
        {
            speedTimer += Time.deltaTime;
            if (speedTimer >= speedIncreaseInterval)
            {
                scrollSpeed *= speedMultiplier;
                speedTimer = 0f;
            }
        }

        // 取得攝影機下界 Y 座標
        float cameraBottomY = Camera.main.transform.position.y - Camera.main.orthographicSize;
        float cameraTopY = Camera.main.transform.position.y + Camera.main.orthographicSize;

        foreach (GameObject plat in platforms)
        {
            // 如果平台超出畫面上方一定距離，則移到相機底下並隨機 X
            if (plat.transform.position.y > cameraTopY + GetSpacing())
            {
                float newX = Random.Range(-xRange, xRange);
                float newY = cameraBottomY - GetSpacing() * 0.5f;
                plat.transform.position = new Vector3(newX, newY, 0);
                // 每次重用時重新決定是否成為水
                UpdatePlatformType(plat);
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
        // 移除舊的 WaterPlatform 標記
        var existing = plat.GetComponent<WaterPlatform>();
        if (existing != null)
            Destroy(existing);

        // 嘗試取得 SpriteRenderer 來變更顏色（若有）
        var sr = plat.GetComponent<SpriteRenderer>();
        if (Random.value < waterProbability)
        {
            // 設為水
            plat.AddComponent<WaterPlatform>();
            if (sr != null)
                sr.color = Color.blue;
        }
        else
        {
            // 一般平台，還原顏色
            if (sr != null)
                sr.color = Color.white;
        }
    }
}
