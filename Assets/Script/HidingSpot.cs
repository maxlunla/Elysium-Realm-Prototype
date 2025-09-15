using UnityEngine;

public class HidingSpot : MonoBehaviour
{
	[Header("Player Reference")]
	public GameObject player;
	private PlayerController playerController;
	public GameObject playerText;

	private bool isPlayerInZone = false;
	private bool isHidden = false;

	void Start()
	{
		playerController = player.GetComponent<PlayerController>();
	}

	void Update()
	{
		if (isPlayerInZone && Input.GetKeyDown(KeyCode.F))
		{
			if (!isHidden) HidePlayer();
			else ExitHide();
		}
	}

	private void HidePlayer()
	{
		isHidden = true;
		player.SetActive(false);

		// แจ้ง AI
		var aiList = FindObjectsOfType<PatrolGuardAI>();
		foreach (var ai in aiList)
		{
			ai.isPlayerHidden = true; // บอกว่า player หายตัว
		}
	}

	private void ExitHide()
	{
		isHidden = false;
		player.SetActive(true);

		// แจ้ง AI
		var aiList = FindObjectsOfType<PatrolGuardAI>();
		foreach (var ai in aiList)
		{
			ai.isPlayerHidden = false; // player กลับมา
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == player)
		{
			isPlayerInZone = true;

			if (playerText != null)
				playerText.SetActive(true); // โชว์ text ตลอดใน zone
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == player)
		{
			isPlayerInZone = false;

			if (playerText != null)
				playerText.SetActive(false); // ออกจาก zone ก็ซ่อน text

			// ถ้า player ซ่อนอยู่แล้วออก trigger ก็ให้โผล่ออกมาอัตโนมัติ
			if (isHidden) ExitHide();
		}
	}
}
