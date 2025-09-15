using UnityEngine;

public class AISight : MonoBehaviour
{
	public PatrolGuardAI ai; // ลิงก์ parent AI

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && ai != null)
		{
			ai.StartChase();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player") && ai != null)
		{
			ai.StopChase();
		}
	}
}