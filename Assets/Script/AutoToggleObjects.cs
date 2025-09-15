using UnityEngine;
using System.Collections;

public class AutoToggleObjects : MonoBehaviour
{
	[Header("Objects to Toggle")]
	public GameObject[] objects; // 7 อัน

	[Header("Timing")]
	public float toggleInterval = 2f; // เวลาสลับ (วินาที)

	private bool toggleEven = true;

	void Start()
	{
		StartCoroutine(ToggleRoutine());
	}

	IEnumerator ToggleRoutine()
	{
		while (true)
		{
			for (int i = 0; i < objects.Length; i++)
			{
				if (toggleEven)
				{
					// เปิดเลขคู่ (Index 1,3,5 ...) เพราะ array เริ่มที่ 0
					objects[i].SetActive((i + 1) % 2 == 0);
				}
				else
				{
					// เปิดเลขคี่ (Index 0,2,4,6 ...)
					objects[i].SetActive((i + 1) % 2 != 0);
				}
			}

			toggleEven = !toggleEven;           // สลับรอบต่อไป
			yield return new WaitForSeconds(toggleInterval); // รอเวลาที่กำหนด
		}
	}
}
