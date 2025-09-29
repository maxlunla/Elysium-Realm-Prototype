using UnityEngine;
using System.Collections.Generic;

// This script will attach to the Reset Switch in the puzzle. The switch will turn all lights back on and update the signal indicators accordingly.
public class ResetSwitch : MonoBehaviour
{
	[Header("Lights and Signals")]
	public List<GameObject> allLights;       // All lights in the puzzle 
	public List<Renderer> allSignalBulbs;    // All signal bulbs (indicators) in the puzzle

	[Header("Materials")]
	public Material onMaterial;              // Red = Light On
	public Material offMaterial;             // Green = Light Off

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

	// Reset all lights to on and update signals
	public void ResetLights()
	{
		// Activeate all lights in the list
		foreach (var light in allLights)
		{
			if (light != null)
				light.SetActive(true);
		}

		// Set all signal bulbs to onMaterial (Red)
		for (int i = 0; i < allSignalBulbs.Count; i++)
		{
			if (allSignalBulbs[i] != null)
			{
				allSignalBulbs[i].material = onMaterial;
			}
		}
	}

	// Show or hide interaction text based on player presence in trigger
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
