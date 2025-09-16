using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// This script manages the player's health, updates the UI, and handles game over state.
public class PlayerHealth : MonoBehaviour
{
	[Header("HP Settings")]
	public int maxHP = 100;				// Maximum health points
	public int currentHP;				// Current health points

	[Header("UI")]
	public TextMeshProUGUI hpText;		// Text to display HP
	public Image hpBar;					// Image to represent HP bar fill (should be of type "Filled")
	public GameObject gameOverText;		// Game Over text object

	void Start()
	{
		currentHP = maxHP;				// Initialize current HP
		UpdateUI();						// Update the UI at start
		gameOverText.SetActive(false);	// Hide Game Over text at start
	}

	void UpdateUI()
	{
		// Update HP text and bar
		hpText.text = "HP: " + currentHP;				// Update HP text
		hpBar.fillAmount = (float)currentHP / maxHP;	// Update HP bar fill
	}

	public void TakeDamage(int damage)
	{
		// Reduce current HP and update UI
		currentHP -= damage;

		// Clamp current HP to not go below 0
		if (currentHP < 0) currentHP = 0;
		UpdateUI();

		// If HP drops to 0 or below, trigger Game Over
		if (currentHP <= 0)
			GameOver();
	}

	void GameOver()
	{
		// Show Game Over text and stop the game
		gameOverText.SetActive(true);
		Time.timeScale = 0f;	// Pause the game
	}

	void Update()
	{
		// Restart the game if player is dead and 'Y' is pressed
		if (currentHP <= 0 && Input.GetKeyDown(KeyCode.Y))
		{
			Time.timeScale = 1f;	// Resume the game
			SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
		}
	}
}