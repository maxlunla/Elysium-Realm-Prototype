using UnityEngine;

// This script allows the player to hide in designated hiding spots.
public class HidingSpot : MonoBehaviour
{
	[Header("Player Reference")]
	public GameObject player;
	private PlayerController playerController;
	public GameObject playerText;

	private bool isPlayerInZone = false;
	private bool isHidden = false;

	void Start()
	{
		playerController = player.GetComponent<PlayerController>();
	}

	void Update()
	{
		// If the player is in the zone and presses F, toggle hiding
		if (isPlayerInZone && Input.GetKeyDown(KeyCode.F))
		{
			if (!isHidden) HidePlayer();	// If not hidden, hide
			else ExitHide();				// If hidden, exit hiding
		}
	}

	// Function to hide the player
	private void HidePlayer()
	{
		isHidden = true;			// Set hidden state to true
		player.SetActive(false);	// Deactivate player object

		// Notify all AI that the player is hidden
		var aiList = FindObjectsOfType<PatrolGuardAI>();
		foreach (var ai in aiList)
		{
			ai.isPlayerHidden = true;	// player is hidden now
		}
	}

	// Function to exit hiding
	private void ExitHide()
	{
		isHidden = false;			// Set hidden state to false
		player.SetActive(true);		// Reactivate player object

		// Notify all AI that the player is no longer hidden
		var aiList = FindObjectsOfType<PatrolGuardAI>();
		foreach (var ai in aiList)
		{
			ai.isPlayerHidden = false;	// player is visible now
		}
	}

	// Trigger detection for entering/exiting the hiding zone
	private void OnTriggerEnter(Collider other)
	{
		// Check if the player entered the zone
		if (other.gameObject == player)
		{
			isPlayerInZone = true;			// Player is in the zone

			if (playerText != null)
				playerText.SetActive(true); // Show text to indicate hiding option
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == player)
		{
			isPlayerInZone = false;				// Player left the zone

			if (playerText != null)
				playerText.SetActive(false);	// Hide the text

			// If the player leaves while hidden, force exit hiding
			if (isHidden) ExitHide();
		}
	}
}
