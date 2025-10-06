using UnityEngine;
using UnityEngine.SceneManagement;

// This script manages the finish door functionality, allowing the player to complete the level and restart the game.
public class FinishDoor : MonoBehaviour
{
	[Header("Player Reference")]
	public GameObject player;					// Reference to the player object
	private PlayerController playerController;	// Reference to the player's controller script
	public GameObject playerText;				// Text to prompt player to finish level
	public GameObject winText;					// Text to display when the player wins

	private bool isPlayerInZone = false;
	private bool isFinished = false;

	void Start()
	{
		playerController = player.GetComponent<PlayerController>();

		if (winText != null)
			winText.SetActive(false);
	}

	void Update()
	{
		// Check if player is in the finish zone and presses F to finish the level
		if (isPlayerInZone && Input.GetKeyDown(KeyCode.F) && !playerController.IsDead && !isFinished)
		{
			player.GetComponent<MeshRenderer>().enabled = false;
			FinishLevel();
		}

		// If the level is finished, allow restart with Y key
		if (isFinished && Input.GetKeyDown(KeyCode.Y))
		{
			RestartLevel();
		}
	}

	private void FinishLevel()
	{
		isFinished = true;

		if (playerText != null)
			playerText.SetActive(false);

		if (winText != null)
			winText.SetActive(true);

		// Disable player controls
		if (playerController != null)
			playerController.enabled = false;
	}

	private void RestartLevel()
	{
		// Reload the current scene to restart the level
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == player)
		{
			isPlayerInZone = true;
			if (playerText != null)
				playerText.SetActive(true);	// Show prompt text when player enters the zone
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == player)
		{
			isPlayerInZone = false;
			if (playerText != null)
				playerText.SetActive(false);	// Hide prompt text when player exits the zone
		}
	}
}
