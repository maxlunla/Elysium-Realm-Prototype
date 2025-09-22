using UnityEngine;

// This script handles player interaction zones for pushing a cube.
public class CubeInteractZone : MonoBehaviour
{
	public PushableCubeSnap cubeScript;	// Reference to the PushableCubeSnap script on the cube
	public bool isLeftZone = true;		// Is this the left interaction zone?
	public GameObject interactText;		// UI text to show when player is in the zone (child of the player)

	private void OnTriggerEnter(Collider other)
	{
		// Check if the player enters the interaction zone, then show the interact text and set the appropriate zone in the cube script
		if (other.CompareTag("Player"))
		{
			if (isLeftZone)
			{
				cubeScript.SetLeftZone(true);
				interactText.SetActive(true);
			}

			else
			{
				cubeScript.SetRightZone(true);
				interactText.SetActive(true);
			}
				
		}
	}

	private void OnTriggerExit(Collider other)
	{
		// Check if the player exits the interaction zone, then hide the interact text and unset the appropriate zone in the cube script
		if (other.CompareTag("Player"))
		{
			if (isLeftZone)
			{
				cubeScript.SetLeftZone(false);
				interactText.SetActive(false);
			}

			else
			{
				cubeScript.SetRightZone(false);
				interactText.SetActive(false);
			}
		}
	}
}
