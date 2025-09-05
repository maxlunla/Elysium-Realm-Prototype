using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Data structure for command entry
[System.Serializable]
public class CommandEntry
{
	public string id;           // "Attack", "Guard", "Skills", "Items", "Escape"
	public Button button;       // Button component
}

// This script manages the command menu UI for selecting actions during battle.
public class CommandMenuController : MonoBehaviour
{
	[Header("Entries (5)")]
	public CommandEntry[] entries;				// Representing the command buttons in UI

	[Header("Skill Menu")]
	public GameObject skillMenuPanel;			// Panel for skill selection

	[SerializeField] public int index = 0;      // Current selected index of action in command
	[SerializeField] public int lastSelectedIndex = 0;  // Store last selected index

	private PartySelector owner;				// For storing the owner party selector
	private BattleActorEntity actorEntity;		// For storing the current actor entity

	private bool locked = false;                // Lock input when processing to avoid multiple inputs

	public Color highlightColor = Color.yellow;	// Color for highlighting selected button
	private Color normalColor = Color.white;    // Normal color for buttons

	public PartySelector partySelector; // assign จาก inspector

	public void SetLocked(bool value)
	{
		// Lock or unlock input
		locked = value;
	}

	public void OpenMenu(PartySelector ownerSelector, BattleActorEntity actorEntity)
	{
		// This function opens the command menu for the specified actor entity
		owner = ownerSelector;				// Set owner
		this.actorEntity = actorEntity;		// Set current actor entity

		gameObject.SetActive(true);			// Show command menu

		// Show available commands of the actor entity
		for (int i = 0; i < entries.Length; i++)
		{
			var cmd = entries[i];		// Get command entry

			if (cmd.button == null) continue;	// Skip if button is not assigned

			// Get Text or TextMeshPro component
			Text textComponent = cmd.button.GetComponentInChildren<Text>();
			TextMeshProUGUI tmpComponent = cmd.button.GetComponentInChildren<TextMeshProUGUI>();

			if (i < actorEntity.commands.Count)
			{
				// Set button text to command display name
				var data = actorEntity.commands[i];
				if (textComponent != null)
					textComponent.text = data.displayName;
				if (tmpComponent != null)
					tmpComponent.text = data.displayName;

				cmd.button.interactable = data.available;
			}
			else
			{
				// No command available, clear text and disable button
				if (textComponent != null)
					textComponent.text = "";
				if (tmpComponent != null)
					tmpComponent.text = "";

				cmd.button.interactable = false;
			}
		}

		// Set index to last selected index
		index = lastSelectedIndex;
		RefreshHighlight();
	}

	public void CloseMenu()
	{
		// This function closes the command menu
		gameObject.SetActive(false);
	}

	void Update()
	{
		// Handle input for navigating and selecting commands
		if (!gameObject.activeSelf) return;		// Skip if menu is not active
		if (locked) return;                     // Skip if input is locked

		if (partySelector.currentState == BattleUIState.CommandSelection)
		{
			if (Input.GetKeyDown(KeyCode.W))
			{
				// Move up
				if (index > 0) index--;
				RefreshHighlight();
			}

			if (Input.GetKeyDown(KeyCode.S))
			{
				// Move down
				if (index < entries.Length - 1) index++;
				RefreshHighlight();
			}

			if (Input.GetKeyDown(KeyCode.Return))
			{
				// Select current command
				Confirm();
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				partySelector.currentState = BattleUIState.PartySelection;

				// Cancel and go back to party selection
				CloseMenu();

				// Reset selection index to first button
				index = 0;
				lastSelectedIndex = 0; // Next time open, start from first button
				RefreshHighlight();

				// Notify owner to go back to party selection
				if (owner != null)
					owner.BackToPartySelection();
			}
		}
	}

	public void RefreshHighlight()
	{
		for (int i = 0; i < entries.Length; i++)
		{
			// Highlight the selected button
			var img = entries[i].button?.GetComponent<Image>();

			if (img != null)
			{
				// Change color based on selection state 
				// (highlightColor for selected, normalColor for others)
				img.color = (i == index) ? highlightColor : normalColor;
			}
		}

		if (entries.Length > 0 && entries[index].button != null)
		{
			// If there are entries, set the selected button in EventSystem
			EventSystem.current.SetSelectedGameObject(null);								// Deselect current
			EventSystem.current.SetSelectedGameObject(entries[index].button.gameObject);	// Select new
		}
	}

	void Confirm()
	{
		// Confirm the selected command
		lastSelectedIndex = index;	// Store last selected index

		var selected = entries[index].id;	// Get selected command ID

		switch (selected)
		{
			// Handle each command accordingly
			case "Attack":
				// Select target for attack
				Debug.Log(actorEntity.characterName + " chose to Attack!");

				SetLocked(true);
				break;

			case "Guard":
				// Make selected actor guard
				Debug.Log(actorEntity.characterName + " chose to Guard!");
				break;

			case "Skills":
				// Open skill menu for skill selection
				// Highlight the selected button in yellow and lock input to avoid multiple inputs
				var img = entries[index].button.GetComponent<Image>();

				if (img != null) img.color = highlightColor;

				SetLocked(true);
				EventSystem.current.SetSelectedGameObject(null);

				// Open skill menu panel.
				partySelector.GoToSkillSelection();
				var skillMenu = skillMenuPanel.GetComponent<SkillMenuController>();
				if (skillMenu != null)
					skillMenu.OpenMenu(owner, this, actorEntity);
				break;

			case "Items":
				// Not available in prototype
				break;

			case "Escape":
				// Make current actor escape from battle
				Debug.Log(actorEntity.characterName + " chose to Escape!");
				break;
		}
	}

	CommandEntry FindEntry(string id)
	{
		foreach (var e in entries)
			if (e.id == id) return e;
		return null;
	}
}