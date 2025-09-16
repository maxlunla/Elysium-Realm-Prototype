using UnityEngine;

// This script handles the AI's sight detection using a trigger collider.
public class AISight : MonoBehaviour
{
	public PatrolGuardAI ai;		// Reference to the PatrolGuardAI script on the same GameObject (parent object)

	// When the player enters the sight trigger, start chasing.
	private void OnTriggerEnter(Collider other)
	{
		// Check if the collider belongs to the player and if the ai reference is set
		if (other.CompareTag("Player") && ai != null)
		{
			ai.StartChase();
		}
	}

	// When the player exits the sight trigger, stop chasing.
	private void OnTriggerExit(Collider other)
	{
		// Check if the collider belongs to the player and if the ai reference is set
		if (other.CompareTag("Player") && ai != null)
		{
			ai.StopChase();
		}
	}
}