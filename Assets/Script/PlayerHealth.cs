using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
	[Header("HP Settings")]
	public int maxHP = 100;
	public int currentHP;

	[Header("UI")]
	public TextMeshProUGUI hpText;
	public Image hpBar;
	public GameObject gameOverText;

	void Start()
	{
		currentHP = maxHP;
		UpdateUI();
		gameOverText.SetActive(false);
	}

	void UpdateUI()
	{
		hpText.text = "HP: " + currentHP;
		hpBar.fillAmount = (float)currentHP / maxHP;
	}

	public void TakeDamage(int damage)
	{
		currentHP -= damage;
		if (currentHP < 0) currentHP = 0;
		UpdateUI();

		if (currentHP <= 0)
			GameOver();
	}

	void GameOver()
	{
		gameOverText.SetActive(true);
		Time.timeScale = 0f; // หยุดเกมชั่วคราว
	}

	void Update()
	{
		if (currentHP <= 0 && Input.GetKeyDown(KeyCode.Y))
		{
			Time.timeScale = 1f; // เปิดเกมกลับมา
			SceneManager.LoadScene(SceneManager.GetActiveScene().name); // รีสตาร์ท Scene
		}
	}
}