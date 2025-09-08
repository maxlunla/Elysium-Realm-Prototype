using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// This script manage the command menu UI for selecting actions during battle.

// Data structure for a command entry in the command menu
[System.Serializable]
public class CommandEntry
{
	public string id;           // "Attack", "Guard", "Skills", "Items", "Escape"
	public Button button;       // Button component
}

public class CommandMenuController : MonoBehaviour
{
	[Header("Entries (5)")]
	public CommandEntry[] entries;						// Array of command entries (5)

	[Header("Skill Menu")]
	public GameObject skillMenuPanel;					// UI panel for skill menu

	[SerializeField] public int index = 0;				// Current selected index
	[SerializeField] public int lastSelectedIndex = 0;  // Last selected index

	private PartySelector owner;						// For storing the owner party selector
	private BattleActorEntity actorEntity;				// For storing the current actor entity

	private bool locked = false;						// For locking input when processing to avoid multiple inputs

	public Color highlightColor = Color.yellow;			// Color for highlighting selected button
	private Color normalColor = Color.white;			// Normal color for buttons

	public PartySelector partySelector;					// Reference to the PartySelector in the scene

	// SetLocked for locking or unlocking input
	public void SetLocked(bool value)
	{
		// Lock or unlock input
		locked = value;
	}

	// Called to open the command menu for a specific actor entity
	public void OpenMenu(PartySelector ownerSelector, BattleActorEntity actorEntity)
	{
		owner = ownerSelector;				// Set owner party selector
		this.actorEntity = actorEntity;		// Set current actor entity

		gameObject.SetActive(true);			// Show command menu

		// Show available commands of the actor entity
		for (int i = 0; i < entries.Length; i++)
		{
			var cmd = entries[i];				// Get command entry

			if (cmd.button == null) continue;	// Skip if button is not assigned

			// Get Text or TextMeshPro component
			Text textComponent = cmd.button.GetComponentInChildren<Text>();
			TextMeshProUGUI tmpComponent = cmd.button.GetComponentInChildren<TextMeshProUGUI>();

			// Set button text and interactable state based on available commands
			if (i < actorEntity.commands.Count)
			{
				var data = actorEntity.commands[i];
				if (textComponent != null)
					textComponent.text = data.displayName;
				if (tmpComponent != null)
					tmpComponent.text = data.displayName;

				cmd.button.interactable = data.available;
			}
			else
			{
				// If no command, clear text and disable button
				if (textComponent != null)
					textComponent.text = "";
				if (tmpComponent != null)
					tmpComponent.text = "";

				cmd.button.interactable = false;
			}
		}

		index = lastSelectedIndex;		// Restore last selected index
		RefreshHighlight();				// Refresh button highlights
	}

	// CloseMenu function to close the command menu
	public void CloseMenu()
	{
		gameObject.SetActive(false);
	}

	void Update()
	{
		if (!gameObject.activeSelf) return;		// Do noting if menu is not active

		// If input is locked, skip processing input
		if (locked)
		{
			locked = false;
			return;		// Skip this frame
		}

		// Handle input only if in CommandSelection state
		if (partySelector.currentState == BattleUIState.CommandSelection)
		{
			if (Input.GetKeyDown(KeyCode.W))
			{
				// Move selection up
				int newIndex = index - 1;

				// Skip unavailable commands
				while (newIndex >= 0)
				{
					if (newIndex < actorEntity.commands.Count && actorEntity.commands[newIndex].available)
					{
						index = newIndex;
						break;
					}
					newIndex--;
				}

				RefreshHighlight();		// Refresh button highlights
			}

			if (Input.GetKeyDown(KeyCode.S))
			{
				// Move selection down
				int newIndex = index + 1;

				// Skip unavailable commands
				while (newIndex < entries.Length)
				{
					if (newIndex < actorEntity.commands.Count && actorEntity.commands[newIndex].available)
					{
						index = newIndex;
						break;
					}
					newIndex++;
				}

				RefreshHighlight();		// Refresh button highlights	
			}

			if (Input.GetKeyDown(KeyCode.Return))
			{
				// Prevent from selecting unavailable command
				if (index >= actorEntity.commands.Count || !actorEntity.commands[index].available)
				{
					Debug.Log("Command not available!");
					return;
				}

				Confirm();		// Confirm selection
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				// Go back to party selection on Escape key
				partySelector.currentState = BattleUIState.PartySelection;

				CloseMenu();			// Close command menu
				index = 0;				// Reset index
				lastSelectedIndex = 0;	// Next time open, start from first button
				RefreshHighlight();		// Refresh button highlights

				// Go back to party selection
				if (owner != null)
					owner.BackToPartySelection();
			}
		}
	}

	// RefreshHighlight to update button colors based on selection and availability
	public void RefreshHighlight()
	{
		// Update button colors
		for (int i = 0; i < entries.Length; i++)
		{
			var img = entries[i].button?.GetComponent<Image>();		// Get button image component

			if (img != null)
			{
				if (i >= actorEntity.commands.Count || !actorEntity.commands[i].available)
				{
					// If command is not available, gray it out
					img.color = Color.gray;
				}
				else
				{
					// Highlight selected button, normal color for others
					img.color = (i == index) ? highlightColor : normalColor;
				}
			}
		}

		if (entries.Length > 0 && entries[index].button != null)
		{
			// Set selected button in EventSystem for keyboard/controller navigation
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(entries[index].button.gameObject);
		}
	}

	// Confirm function to handle command selection
	void Confirm()
	{
		lastSelectedIndex = index;						// Store last selected index
		var selected = entries[index].id;				// Get selected command ID
		var actionData = actorEntity.commands[index];	// Get action data

		// Handle command based on selection
		switch (selected)
		{
			case "Attack":
				// Select target for attack
				locked = true;		// Lock input to avoid multiple inputs while targeting
				FindFirstObjectByType<TargetSelector>().StartTargeting(actorEntity, null, actionData);

				Debug.Log(actorEntity.characterName + " chose to Attack!");
				break;

			case "Guard":
				// Make selected actor guard
				locked = true;      // Lock input to avoid multiple inputs while targeting
				FindFirstObjectByType<TargetSelector>().StartTargeting(actorEntity, null, actionData);

				Debug.Log(actorEntity.characterName + " chose to Guard!");
				break;

			case "Skills":
				// Open skill menu for skill selection and change state to SkillSelection
				var img = entries[index].button.GetComponent<Image>();		// Get button image component

				if (img != null) img.color = highlightColor;				// If image component exists, highlight it

				SetLocked(true);											// Lock input to avoid multiple inputs while targeting
				EventSystem.current.SetSelectedGameObject(null);			// Deselect current button

				partySelector.GoToSkillSelection();							// Change state to SkillSelection
				var skillMenu = skillMenuPanel.GetComponent<SkillMenuController>();
				
				if (skillMenu != null)
					skillMenu.OpenMenu(owner, this, actorEntity);           // Open skill menu if component exists
				break;

			case "Items":
				// Not available in prototype
				break;

			case "Escape":
				// Make current actor escape from battle
				locked = true;      // Lock input to avoid multiple inputs while targeting
				FindFirstObjectByType<TargetSelector>().StartTargeting(actorEntity, null, actionData);

				Debug.Log(actorEntity.characterName + " chose to Escape!");
				break;
		}
	}

	// FindEntry function to find a CommandEntry by its ID
	CommandEntry FindEntry(string id)
	{
		foreach (var e in entries)
			if (e.id == id) return e;
		return null;
	}
}