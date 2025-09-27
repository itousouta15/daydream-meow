using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    private Rigidbody2D rb;

    // 判斷玩家是否站在平台上
    private bool isOnPlatform = false;
    // 指向主選單的 Canvas（在 Inspector 指定）
    public Canvas mainMenuCanvas;
    // 玩家超出攝影機上界後的緩衝距離（避免誤觸發）
    public float topBuffer = 0.5f;
    // 是否已經回到主選單，避免重複觸發
    private bool returnedToMenu = false;

    void Start() 
    {
        rb = GetComponent<Rigidbody2D>();
        // 鎖定 Rigidbody2D 的旋轉，避免因碰撞或力矩而旋轉角色
        // 也可以在 Unity Inspector 的 Rigidbody2D component 中勾選 Constraints > Freeze Rotation
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update() 
    {
        float moveInput = 0f;
        var keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                moveInput = -1f;
            else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                moveInput = 1f;
        }

        // 只控制水平速度，不強制設y
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        rb.linearVelocity = velocity;

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
                ReturnToMainMenu();
            }
        }
    }

    // 只要碰到平台就算站在平台上

    void OnCollisionStay2D(Collision2D collision)
    {
        // 如果碰到的是水平台（WaterPlatform 組件），直接死亡
        if (collision.gameObject.GetComponent<WaterPlatform>() != null)
        {
            // 直接回到主選單
            ReturnToMainMenu();
            return;
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            // 只要有一個碰撞點在玩家下方就算站在平台上
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.point.y < transform.position.y - 0.1f)
                {
                    isOnPlatform = true;
                    return;
                }
            }
        }
        isOnPlatform = false;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
        }
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

    // 供外部呼叫以重置 "已回到主選單" 的旗標，方便重新開始遊戲
    public void ResetReturnedFlag()
    {
        returnedToMenu = false;
    }
}
