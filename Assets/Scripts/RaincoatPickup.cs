using UnityEngine;

// 簡單的雨衣拾取物：當玩家的 Collider 進入 trigger 時，呼叫 PlayerController.EquipRaincoat() 並摧毀自己。
[DisallowMultipleComponent]
public class RaincoatPickup : MonoBehaviour
{
	// 如果你希望只讓玩家撿取，可以在 Inspector 指定 playerTag（預設為 "Player"）
	public string playerTag = "Player";

	// 可選：進入觸發的時候播放音效或特效（留空代表不播放）
	public AudioClip pickupSfx;
	public float destroyDelay = 0f; // 若需要延遲摧毀

	private AudioSource audioSource;

	void Awake()
	{
		if (pickupSfx != null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
			audioSource.clip = pickupSfx;
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other == null) return;

		// 若設定了 playerTag，先檢查 tag
		if (!string.IsNullOrEmpty(playerTag))
		{
			// 允許子物件或父物件帶有該 tag
			bool tagMatch = other.CompareTag(playerTag) || (other.attachedRigidbody != null && other.attachedRigidbody.gameObject.CompareTag(playerTag));
			if (!tagMatch)
				return;
		}

		// 嘗試在其他物件或其父物件上取得 PlayerController
		PlayerController pc = other.GetComponent<PlayerController>();
		if (pc == null && other.attachedRigidbody != null)
			pc = other.attachedRigidbody.GetComponent<PlayerController>();
		if (pc == null)
			pc = other.GetComponentInParent<PlayerController>();

		if (pc != null)
		{
			// 呼叫 EquipRaincoat（PlayerController 的方法已設為 public）
			pc.EquipRaincoat();

			if (audioSource != null)
			{
				audioSource.Play();
				// 若沒有額外 destroyDelay，等待音效長度後再刪除
				float delay = destroyDelay > 0f ? destroyDelay : audioSource.clip != null ? audioSource.clip.length : 0f;
				if (delay <= 0f)
					Destroy(gameObject);
				else
					Destroy(gameObject, delay);
			}
			else
			{
				if (destroyDelay <= 0f)
					Destroy(gameObject);
				else
					Destroy(gameObject, destroyDelay);
			}
		}
	}
}
