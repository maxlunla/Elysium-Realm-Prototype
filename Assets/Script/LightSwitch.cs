using UnityEngine;
using System.Collections.Generic;

public class LightSwitch : MonoBehaviour
{
	[Header("Lights Controlled by This Switch")]
	public List<GameObject> lights; // ไฟที่ switch นี้ควบคุม (แต่ละ switch assign ใน inspector ต่างกัน)

	[Header("Signal Bulbs (Indicators)")]
	public List<Renderer> signalBulbs; // หลอด signal 5 อัน (เรียงตาม L1-L5 ของ puzzle)

	[Header("Materials")]
	public Material onMaterial;     // สีแดง = Light เปิด
	public Material offMaterial;    // สีเขียว = Light ปิด

	[Header("UI / Player")]
	public GameObject interactText;
	public PlayerController playerController;

	private bool playerInTrigger = false;

	void Start()
	{
		UpdateSignals(); // อัปเดต signal ตอนเริ่ม
		if (interactText != null) interactText.SetActive(false);
	}

	void Update()
	{
		if (playerInTrigger && Input.GetKeyDown(KeyCode.F) && playerController.IsDead == false)
		{
			ToggleLights();
			UpdateSignals();
		}
	}

	// Toggle เฉพาะไฟใน list ของ switch
	private void ToggleLights()
	{
		foreach (var lightObj in lights)
		{
			if (lightObj != null)
				lightObj.SetActive(!lightObj.activeSelf); // เปิด/ปิด
		}
	}

	// อัปเดต signal ให้ตรงกับสถานะไฟ
	private void UpdateSignals()
	{
		for (int i = 0; i < signalBulbs.Count; i++)
		{
			if (signalBulbs[i] != null)
			{
				// เช็คว่ามีไฟตัวนี้ใน list ของ switch หรือไม่
				if (i < lights.Count && lights[i] != null)
				{
					signalBulbs[i].material = lights[i].activeSelf ? onMaterial : offMaterial;
				}
				else
				{
					// ถ้าไฟไม่มีใน list ของ switch ให้ default เป็น off
					signalBulbs[i].material = offMaterial;
				}
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
