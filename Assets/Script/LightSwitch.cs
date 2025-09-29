using UnityEngine;
using System.Collections.Generic;

// This script will attach to each Light Switch in the puzzle. The switch will control specific lights and update the signal indicators accordingly.
public class LightSwitch : MonoBehaviour
{
	[Header("Lights Controlled by This Switch")]
	public List<GameObject> lights;			// List of lights this switch controls toggle 

	[Header("Signal Bulbs (Indicators)")]
	public List<Renderer> signalBulbs;		// List of signal bulbs (indicators) in the scene

	[Header("Materials")]
	public Material onMaterial;				// Red = Light On (Danger)
	public Material offMaterial;			// Green = Light Off (Safe)

	[Header("UI / Player")]
	public GameObject interactText;
	public PlayerController playerController;

	private bool playerInTrigger = false;

	void Start()
	{
		UpdateSignals();
		if (interactText != null) interactText.SetActive(false);
	}

	void Update()
	{
		// Check if player is in trigger and presses F
		if (playerInTrigger && Input.GetKeyDown(KeyCode.F) && playerController.IsDead == false)
		{
			ToggleLights();
			UpdateSignals();
		}
	}

	// Toggle the state of the lights this switch controls
	private void ToggleLights()
	{
		foreach (var lightObj in lights)
		{
			if (lightObj != null)
				lightObj.SetActive(!lightObj.activeSelf);	// Toggle the light state 
		}
	}

	// Update the signal indicators based on the current state of the lights
	private void UpdateSignals()
	{
		for (int i = 0; i < signalBulbs.Count; i++)
		{
			if (signalBulbs[i] != null)
			{
				// Check if the light exists in the list and update the signal accordingly
				if (i < lights.Count && lights[i] != null)
				{
					signalBulbs[i].material = lights[i].activeSelf ? onMaterial : offMaterial;
				}
				else
				{
					// If no corresponding light, set to off (safe)
					signalBulbs[i].material = offMaterial;
				}
			}
		}
	}

	// Show interact text when player enters trigger
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
