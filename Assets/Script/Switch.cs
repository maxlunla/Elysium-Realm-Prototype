using UnityEngine;

// This script allows the player to interact with a switch that hides certain objects for 5 seconds. (Press 'F' to activate)
public class Switch : MonoBehaviour
{
	[Header("Objects to Hide")]
	public GameObject[] objectsToHide;		// list of objects to hide when switch is activated

	public GameObject interactText;			// text to show when player is in range to interact with the switch (text is child of player)

	private bool playerInTrigger = false;	// to check if player is in trigger area of the switch 
	private bool isOnCooldown = false;		// to prevent multiple activations during cooldown after activation

	void Update()
	{
		// Check if player is in trigger and presses 'F' to activate the switch and not on cooldown
		if (playerInTrigger && !isOnCooldown && Input.GetKeyDown(KeyCode.F))
		{
			ActivateSwitch();		// activate the switch
			CloseInteractText();	// close the interact text
		}
	}

	void ActivateSwitch()
	{
		isOnCooldown = true;	// set cooldown to true to prevent multiple activations during cooldown

		// Hide all specified objects
		foreach (GameObject obj in objectsToHide)
		{
			obj.SetActive(false);
		}

		StartCoroutine(ReactivateObjects());	// start coroutine to reactivate objects after 5 seconds
	}

	// Show interact text if player is in trigger and not on cooldown
	private void OnTriggerEnter(Collider other)
	{
		// Check if the player entered the trigger
		if (other.CompareTag("Player"))
		{
			playerInTrigger = true;

			// Show interact text only if not on cooldown
			if (!isOnCooldown)
				interactText.SetActive(true);
		}
	}

	// Hide interact text if player exits trigger
	private void OnTriggerExit(Collider other)
	{
		// Check if the player exited the trigger
		if (other.CompareTag("Player"))
		{
			playerInTrigger = false;		// set playerInTrigger to false
			interactText.SetActive(false);	// hide interact text
		}
	}

	// Coroutine to reactivate objects after 5 seconds
	private System.Collections.IEnumerator ReactivateObjects()
	{
		yield return new WaitForSeconds(5f);		// wait for 5 seconds
		foreach (GameObject obj in objectsToHide)	// reactivate all specified objects
		{
			obj.SetActive(true);
		}
		
		isOnCooldown = false;		// reset cooldown to allow activation again

		// If player is still in trigger, show interact text again
		if (playerInTrigger)
			OpenInteractText();
	}

	// Functions to open and close interact text
	private void CloseInteractText()
	{
		interactText.SetActive(false);
	}

	private void OpenInteractText()
	{
		interactText.SetActive(true);
	}
}
