using System.Collections;
using UnityEngine;

// This script allows the player to hide in designated hiding spots and heal over time while hidden.
public class HealingShadow : MonoBehaviour
{
	[Header("Player Reference")]
	public GameObject player;						// Reference to the player object
	private PlayerController playerController;		// Reference to the player's controller script
	public GameObject playerText;					// UI text to show when player can hide

	[Header("Healing Settings")]
	public float interval = 0.2f;					// Interval between each heal tick
	public int healAmount = 2;						// Amount of health to restore each tick

	private bool isPlayerInZone = false;			// Is the player in the hiding zone?
	private bool isHidden = false;					// Is the player currently hidden?
	private Coroutine healCoroutine;				// Reference to the healing coroutine

	void Start()
	{
		playerController = player.GetComponent<PlayerController>();
	}

	void Update()
	{
		// If the player is in the zone and presses F, toggle hiding
		if (isPlayerInZone && Input.GetKeyDown(KeyCode.F) && playerController.IsDead == false)
		{
			if (!isHidden) HidePlayer();    // If not hidden, hide
			else ExitHide();                // If hidden, exit hiding
		}
	}

	// Function to hide the player
	private void HidePlayer()
	{
		playerController.isInShadow = true; // Player is in shadow when hiding, so set this to true to prevent being killed by deadly light
		isHidden = true;                    // Set hidden state to true

		player.layer = LayerMask.NameToLayer("HiddenPlayer");

		// Disable player colliders to prevent interaction
		var colliders = player.GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
			col.enabled = false;

		// Dable player renderers to make them invisible
		var renderers = player.GetComponentsInChildren<Renderer>();
		foreach (var r in renderers)
			r.enabled = false;

		// Start Heal Over Time
		if (healCoroutine == null)
			healCoroutine = StartCoroutine(HealOverTime());

		// Notify all AI that the player is hidden
		var aiList = FindObjectsOfType<PatrolGuardAI>();
		foreach (var ai in aiList)
		{
			ai.isPlayerHidden = true;   // player is hidden now
		}
	}

	// Function to exit hiding
	private void ExitHide()
	{
		playerController.isInShadow = false; // Player is no longer in shadow when exiting hiding
		isHidden = false;                   // Set hidden state to false

		// Enable player colliders and renderers to make them visible and interactive again
		var colliders = player.GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
			col.enabled = true;

		// Enable player renderers to make them visible
		var renderers = player.GetComponentsInChildren<Renderer>();
		foreach (var r in renderers)
			r.enabled = true;

		player.layer = LayerMask.NameToLayer("Default");

		// Stop healing coroutine if it's running
		if (healCoroutine != null)
		{
			StopCoroutine(healCoroutine);
			healCoroutine = null;
		}

		// Notify all AI that the player is no longer hidden
		var aiList = FindObjectsOfType<PatrolGuardAI>();
		foreach (var ai in aiList)
		{
			ai.isPlayerHidden = false;  // player is visible now
		}
	}

	// Trigger detection for entering/exiting the hiding zone
	private void OnTriggerEnter(Collider other)
	{
		// Check if the player entered the zone
		if (other.gameObject == player)
		{
			isPlayerInZone = true;          // Player is in the zone

			if (playerText != null)
				playerText.SetActive(true); // Show text to indicate hiding option
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == player)
		{
			isPlayerInZone = false;             // Player left the zone

			if (playerText != null)
				playerText.SetActive(false);    // Hide the text

			// If the player leaves while hidden, force exit hiding
			if (isHidden) ExitHide();
		}
	}

	// Coroutine to heal the player over time while hidden
	private IEnumerator HealOverTime() 
	{
		while (isHidden && !playerController.IsDead)
		{
			player.GetComponent<PlayerHealth>().Heal(healAmount);
			yield return new WaitForSeconds(interval);
		}
		healCoroutine = null;
	}
}
