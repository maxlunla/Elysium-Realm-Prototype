using UnityEngine;
using UnityEngine.SceneManagement; // สำหรับ reload scene

public class FinishDoor : MonoBehaviour
{
	[Header("Player Reference")]
	public GameObject player;
	private PlayerController playerController;
	public GameObject playerText;       // ข้อความ "Finish [F]"
	public GameObject winText;          // ข้อความ "YOU WIN Press [Y] to play again"

	private bool isPlayerInZone = false;
	private bool isFinished = false;

	void Start()
	{
		playerController = player.GetComponent<PlayerController>();

		if (winText != null)
			winText.SetActive(false); // ซ่อนข้อความตอนเริ่มเกม
	}

	void Update()
	{
		// เมื่อผู้เล่นอยู่ในโซนและกด F เพื่อจบด่าน
		if (isPlayerInZone && Input.GetKeyDown(KeyCode.F) && !playerController.IsDead && !isFinished)
		{
			player.GetComponent<MeshRenderer>().enabled = false;
			FinishLevel();
		}

		// ถ้าจบด่านแล้วและกด Y จะเริ่มใหม่
		if (isFinished && Input.GetKeyDown(KeyCode.Y))
		{
			RestartLevel();
		}
	}

	private void FinishLevel()
	{
		isFinished = true;

		if (playerText != null)
			playerText.SetActive(false);

		if (winText != null)
			winText.SetActive(true);

		// หยุดการควบคุมของผู้เล่น (ถ้ามีระบบ movement)
		if (playerController != null)
			playerController.enabled = false;

		// สามารถใส่ effect เพิ่ม เช่น particle หรือเสียง
		Debug.Log("YOU WIN!");
	}

	private void RestartLevel()
	{
		// โหลด scene ปัจจุบันใหม่
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == player)
		{
			isPlayerInZone = true;
			if (playerText != null)
				playerText.SetActive(true); // แสดงข้อความ "Finish [F]"
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == player)
		{
			isPlayerInZone = false;
			if (playerText != null)
				playerText.SetActive(false); // ซ่อนข้อความเมื่อออก
		}
	}
}
