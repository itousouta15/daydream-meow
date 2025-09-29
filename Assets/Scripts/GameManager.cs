using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerController playerController;
    [Header("Audio")]
    public AudioClip bgmClip;
    private AudioSource bgmSource;

    void Start()
    {
        playerController = UnityEngine.Object.FindAnyObjectByType<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("找不到 PlayerController阿幹");
        }

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
        if (playerController != null)
        {
            playerController.UpdateLivesText();
        }
    }
}
