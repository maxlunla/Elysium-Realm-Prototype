using UnityEngine;
using System.Collections.Generic;

public class ResetSwitch : MonoBehaviour
{
	[Header("Lights and Signals")]
	public List<GameObject> allLights;       // ไฟทั้งหมดใน puzzle
	public List<Renderer> allSignalBulbs;    // signal ทั้งหมด (เรียงตาม L1-L5)

	[Header("Materials")]
	public Material onMaterial;              // สีแดง = Light เปิด
	public Material offMaterial;             // สีเขียว = Light ปิด

	[Header("UI / Player")]
	public GameObject interactText;
	public PlayerController playerController;

	private bool playerInTrigger = false;

	private void Start()
	{
		if (interactText != null) interactText.SetActive(false);
	}

	private void Update()
	{
		if (playerInTrigger && Input.GetKeyDown(KeyCode.F) && playerController.IsDead == false)
		{
			ResetLights();
		}
	}

	public void ResetLights()
	{
		// เปิดทุกไฟ
		foreach (var light in allLights)
		{
			if (light != null)
				light.SetActive(true);
		}

		// อัปเดต signal ให้ตรงกับไฟ
		for (int i = 0; i < allSignalBulbs.Count; i++)
		{
			if (allSignalBulbs[i] != null)
			{
				allSignalBulbs[i].material = onMaterial; // สีแดง = เปิด
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			playerInTrigger = true;
			if (interactText != null) interactText.SetActive(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			playerInTrigger = false;
			if (interactText != null) interactText.SetActive(false);
		}
	}
}
