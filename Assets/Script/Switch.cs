using UnityEngine;

public class Switch : MonoBehaviour
{
	[Header("Objects to Hide")]
	public GameObject[] objectsToHide; // วัตถุที่จะหายไป

	public GameObject interactText;

	private bool playerInTrigger = false;
	private bool isOnCooldown = false; // กันกดซ้ำระหว่าง coroutine

	void Update()
	{
		if (playerInTrigger && !isOnCooldown && Input.GetKeyDown(KeyCode.F))
		{
			ActivateSwitch();
			CloseInteractText();
		}
	}

	void ActivateSwitch()
	{
		isOnCooldown = true; // ปิดการใช้งานจนกว่า coroutine จะเสร็จ

		foreach (GameObject obj in objectsToHide)
		{
			obj.SetActive(false); // ทำให้วัตถุหายไป
		}

		StartCoroutine(ReactivateObjects());
	}

	// ตรวจว่า Player อยู่ใน Trigger
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			playerInTrigger = true;

			// แสดงข้อความเฉพาะตอนที่ไม่ cooldown
			if (!isOnCooldown)
				interactText.SetActive(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			playerInTrigger = false;
			interactText.SetActive(false);
		}
	}

	private System.Collections.IEnumerator ReactivateObjects()
	{
		yield return new WaitForSeconds(5f); // รอ 5 วินาที
		foreach (GameObject obj in objectsToHide)
		{
			obj.SetActive(true); // ทำให้วัตถุกลับมา
		}
		// เปิดใช้งานใหม่
		isOnCooldown = false;

		// แสดงข้อความใหม่ถ้ายังอยู่ใน trigger
		if (playerInTrigger)
			OpenInteractText();
	}

	private void CloseInteractText()
	{
		interactText.SetActive(false);
	}

	private void OpenInteractText()
	{
		interactText.SetActive(true);
	}
}
