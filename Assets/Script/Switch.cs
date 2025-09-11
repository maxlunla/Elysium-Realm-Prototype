using UnityEngine;

public class Switch : MonoBehaviour
{
	[Header("Objects to Hide")]
	public GameObject[] objectsToHide; // วัตถุที่จะหายไป

	public GameObject interactText;

	private bool playerInTrigger = false;

	void Update()
	{
		if (playerInTrigger && Input.GetKeyDown(KeyCode.F))
		{
			ActivateSwitch();
		}
	}

	void ActivateSwitch()
	{
		foreach (GameObject obj in objectsToHide)
		{
			obj.SetActive(false); // ทำให้วัตถุหายไป
		}
	}

	// ตรวจว่า Player อยู่ใน Trigger
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			playerInTrigger = true;
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
}
