using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Script for NPC interaction with dialogue system.
public class NPC_Interaction : MonoBehaviour
{
	[Header("World-Space Prompt")]
	public GameObject interactPrompt;			// The world-space canvas with start interact prompt (e.g. [F] Talk)
	public float interactDistance = 2.5f;		// Distance check if don't use trigger collider.

	[Header("Screen-Space UI")]
	public GameObject namePanel;				// Panel with NPC name (Screen Space)
	public TextMeshProUGUI nameText;			// TMP text in Name Panel
	public GameObject dialoguePanel;			// Panel with dialogue (Screen Space)
	public TextMeshProUGUI dialogueText;		// TMP text in Dialogue Panel
	public GameObject optionPanel;				// Panel with options choice buttons (Screen Space)
	public Button option1Button;				// Choice 1
	public Button option2Button;				// Choice 2
	[TextArea] public string[] lines;			// Dialogue lines

	[Header("Typing")]
	public float typingSpeed = 0.03f;			// Typewriter speed
	public KeyCode skipKey = KeyCode.Space;		// Key to skip typewriter effect

	[Header("References")]
	public string NPCName = "NPC Name";			// NPC name to show in name panel
	public Animator npcAnimator;				// NPC animator with an idle/talking animation
	public string talkTrigger = "TalkTrigger";	// Trigger parameter in NPC animator
	public Transform lookAtPivot;				// Pivot to look at the player (e.g. head)
	public Player_Movement_Script player;		// Reference to player script
	public Transform playerTransform;			// Player transform
	public Third_Person_Camera cameraController;	// Reference to camera controller script

	// Internal
	private bool playerInside = false;			// Is player inside trigger
	private bool isTalking = false;				// Is NPC talking
	private bool isTyping = false;				// Is typewriter effect running
	private int currentLineIndex = 0;			// Current line index
	private Collider triggerCol;				// Trigger collider

	void Awake()
	{
		triggerCol = GetComponent<Collider>();					// Get trigger collider
		if (triggerCol != null) triggerCol.isTrigger = true;	// Ensure it's a trigger
	}

	void Start()
	{
		// Initial UI state
		if (interactPrompt) interactPrompt.SetActive(false);
		if (dialoguePanel) dialoguePanel.SetActive(false);
		if (optionPanel) optionPanel.SetActive(false);
		if (namePanel) namePanel.SetActive(false);

		// Add button listeners
		if (option1Button) option1Button.onClick.AddListener(() => ChooseOption(1));
		if (option2Button) option2Button.onClick.AddListener(() => ChooseOption(2));

		// Set NPC name
		if (nameText) nameText.text = NPCName;
	}

	void Update()
	{
		// Show/Hide interact prompt based on trigger + not talking to NPC
		if (interactPrompt) interactPrompt.SetActive(playerInside && !isTalking);

		// Start conversation when player presses F
		if (playerInside && !isTalking && Input.GetKeyDown(KeyCode.F))
		{
			StartConversation();
		}

		if (!isTalking) return; // Only proceed if in conversation

		// Skip typewriter to instant full line (Space or Left Click)
		if (isTyping && (Input.GetKeyDown(skipKey) || Input.GetMouseButtonDown(0)))
		{
			// Stop typewriter and show full line
			StopAllCoroutines();
			dialogueText.text = lines[currentLineIndex];	// Show full line
			isTyping = false;	// No longer typing

			// If last line, show options
			if (currentLineIndex == lines.Length - 1)
				optionPanel.SetActive(true);
		}
		else if (!isTyping && optionPanel.activeSelf == false)
		{
			// Advance to next line or show options (if last line)
			if (Input.GetKeyDown(skipKey) || Input.GetMouseButtonDown(0))
				NextLineOrShowOptions();
		}

		// Keyboard shortcuts for options
		if (optionPanel.activeSelf)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1)) ChooseOption(1);
			if (Input.GetKeyDown(KeyCode.Alpha2)) ChooseOption(2);
		}

		// Make player and NPC face each other smoothly
		if (isTalking && playerTransform != null)
		{
			// Player to NPC
			Vector3 dirToNpc = (lookAtPivot != null ? lookAtPivot.position : transform.position) - playerTransform.position;
			dirToNpc.y = 0;

			// Smoothly rotate player to face NPC
			if (dirToNpc.sqrMagnitude > 0.001f)
			{
				Quaternion targetRot = Quaternion.LookRotation(dirToNpc);
				playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRot, Time.deltaTime * 3f);
			}

			// NPC to player
			Vector3 dirToPlayer = playerTransform.position - transform.position;
			dirToPlayer.y = 0;

			// Smoothly rotate NPC to face player
			if (dirToPlayer.sqrMagnitude > 0.001f)
			{
				Quaternion npcTargetRot = Quaternion.LookRotation(dirToPlayer);
				transform.rotation = Quaternion.Slerp(transform.rotation, npcTargetRot, Time.deltaTime * 3f);
			}
		}
	}

	// Start the conversation
	void StartConversation()
	{
		isTalking = true;		// Set talking state
		currentLineIndex = 0;	// Reset line index

		// Stop player movement and camera input
		if (player) player.canMove = false;
		if (cameraController) cameraController.SetInputEnabled(true);
		if (cameraController) cameraController.isInDialog = true;

		// Play NPC talk animation once
		if (npcAnimator != null && !string.IsNullOrEmpty(talkTrigger))
			npcAnimator.SetTrigger(talkTrigger);

		// Open dialogue UI and start typewriter
		if (namePanel) namePanel.SetActive(true);
		if (dialoguePanel) dialoguePanel.SetActive(true);
		if (optionPanel) optionPanel.SetActive(false);

		ShowCurrentLine();
	}

	void ShowCurrentLine()
	{
		// Checks before starting typewriter
		if (dialogueText == null || lines == null || lines.Length == 0)
			return;

		dialogueText.text = "";			// Clear current text
		optionPanel.SetActive(false);	// Hide options
		StartCoroutine(TypeLine(lines[currentLineIndex]));	// Start typewriter effect
	}

	// Typewriter effect coroutine
	IEnumerator TypeLine(string line)
	{
		isTyping = true;	// Set typing state

		// Type each character with delay
		foreach (char c in line)
		{
			dialogueText.text += c;
			yield return new WaitForSeconds(typingSpeed);
		}

		isTyping = false;	// Typing finished

		// If this is the last line, show options automatically
		if (currentLineIndex == lines.Length - 1)
			optionPanel.SetActive(true);
	}

	// Advance to next line or show options if at the end
	void NextLineOrShowOptions()
	{
		if (currentLineIndex < lines.Length - 1)
		{
			// More lines available
			currentLineIndex++;
			ShowCurrentLine();
		}
		else
		{
			// No more lines, show options
			optionPanel.SetActive(true);
		}
	}

	// Handle option selection
	void ChooseOption(int index)
	{
		if (index == 1)
		{
			// Enter combat scene
		}
		else if (index == 2)
		{
			// End conversation
			EndConversation();
		}
	}

	// End the conversation and restore player control
	void EndConversation()
	{
		isTalking = false;

		// Close dialogue UI
		if (namePanel) namePanel.SetActive(false);
		if (dialoguePanel) dialoguePanel.SetActive(false);
		if (optionPanel) optionPanel.SetActive(false);

		// Restore player and camera
		if (player) player.canMove = true;
		if (cameraController) cameraController.SetInputEnabled(true);
		if (cameraController) cameraController.isInDialog = false;
	}

	// Trigger detection for player entering/exiting interaction zone
	void OnTriggerEnter(Collider other)
	{
		// Check if player entered
		if (other.CompareTag("Player"))
			playerInside = true;
	}

	void OnTriggerExit(Collider other)
	{
		// Check if player exited
		if (other.CompareTag("Player"))
			playerInside = false;
	}
}
