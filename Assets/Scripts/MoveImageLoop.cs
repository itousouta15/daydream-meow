using UnityEngine;

public class MoveUpLoop : MonoBehaviour
{
    public float speed = 2f;          // 移動速度
    public float targetY = 10f;       // 目標 y 座標
    private Vector3 startPos;         // 起始位置

    void Start()
    {
        // 紀錄起始位置
        startPos = transform.position;
    }

    void Update()
    {
        // 沿 y 軸往上移動
        transform.position += Vector3.up * speed * Time.deltaTime;

        // 到達或超過目標y，回到起始點
        if (transform.position.y >= targetY)
        {
            transform.position = startPos;
        }
    }
}
