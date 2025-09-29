using UnityEngine;

[DisallowMultipleComponent]
public class RaincoatPickup : MonoBehaviour
{
	public string playerTag = "Player";

	public AudioClip pickupSfx;
	public float destroyDelay = 0f;

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

		if (!string.IsNullOrEmpty(playerTag))
		{
			bool tagMatch = other.CompareTag(playerTag) || (other.attachedRigidbody != null && other.attachedRigidbody.gameObject.CompareTag(playerTag));
			if (!tagMatch)
				return;
		}

		PlayerController pc = other.GetComponent<PlayerController>();
		if (pc == null && other.attachedRigidbody != null)
			pc = other.attachedRigidbody.GetComponent<PlayerController>();
		if (pc == null)
			pc = other.GetComponentInParent<PlayerController>();

		if (pc != null)
		{
			pc.EquipRaincoat();

			if (audioSource != null)
			{
				audioSource.Play();
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
