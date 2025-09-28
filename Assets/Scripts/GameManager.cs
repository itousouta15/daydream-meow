using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerController playerController;
    [Header("Audio")]
    public AudioClip bgmClip; // 背景 BGM: On The Flip - The Grey Room _ Density & Time
    private AudioSource bgmSource;

    void Start()
    {
        // 尋找場景中已有的 PlayerController
        playerController = UnityEngine.Object.FindAnyObjectByType<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("找不到 PlayerController");
        }

        // 建立 BGM AudioSource
        if (bgmClip != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = 0.3f;
            bgmSource.Play();
        }
    }

    void SomeGameLogic()
    {
        // 需要在遊戲邏輯改變生命數並更新顯示時呼叫
        if (playerController != null)
        {
            // 呼叫更新 TMP 顯示
            playerController.UpdateLivesText();
        }
    }
}
