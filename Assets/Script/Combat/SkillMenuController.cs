using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// Data structure for each skill entry in the menu
[System.Serializable]
public class SkillEntry
{
	public GameObject panel;				// Panel for highlighting selection
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI tpText;
	public TextMeshProUGUI mpText;
	public TextMeshProUGUI cooldownText;
}

// This script manages the skill selection menu during battle
public class SkillMenuController : MonoBehaviour
{
	[Header("Skill List (5)")]
	public List<SkillEntry> entries;			// List of skill entries in the menu (max 5)

	[Header("Description UI")]
	public TextMeshProUGUI descriptionName;
	public TextMeshProUGUI descriptionType;
	public TextMeshProUGUI descriptionText;

	private int index = 0;						// Current selected skill index
	private PartySelector owner;				// Owner party selector
	private CommandMenuController commandMenu;	// Reference to command menu controller
	private BattleActorEntity actorEntity;		// Current actor entity

	public GameObject descriptionPanel;

	public void OpenMenu(PartySelector ownerSelector, CommandMenuController cmdMenu, BattleActorEntity actorEntity)
	{
		// Open the skill menu for the specified actor entity
		owner = ownerSelector;				// Set owner
		commandMenu = cmdMenu;				// Set command menu reference
		this.actorEntity = actorEntity;		// Set current actor entity

		gameObject.SetActive(true);			// Show skill menu

		if (descriptionPanel != null) descriptionPanel.SetActive(true);
		index = 0;		// Reset index

		// Populate skill entries based on actor's skills from BattleActorEntity
		for (int i = 0; i < entries.Count; i++)
		{
			if (i < actorEntity.skills.Count)
			{
				// Show skill entry if skill exists at this index
				var skill = actorEntity.skills[i];
				var entry = entries[i];

				// Populate details in the entry UI
				if (entry.nameText != null) entry.nameText.text = skill.displayName;
				if (entry.tpText != null) entry.tpText.text = skill.tpCost.ToString() + "TP";
				if (entry.mpText != null) entry.mpText.text = skill.mpCost.ToString() + "MP";
				if (entry.cooldownText != null)
				{
					if (skill.cooldown > 0)
					{
						string suffix = skill.cooldown == 1 ? " Turn" : " Turns";
						entry.cooldownText.text = skill.cooldown.ToString() + suffix;
					}
					else
					{
						entry.cooldownText.text = "";
					}
				}
			}
		}

		RefreshHighlight();		// Highlight the first entry
		RefreshDescription();	// Show description of the first skill selected
	}

	public void CloseMenu()
	{
		// Close the skill menu
		gameObject.SetActive(false);

		// Hide description panel and unlock command menu
		if (descriptionPanel != null)
			descriptionPanel.SetActive(false);
		if (commandMenu != null)
			commandMenu.SetLocked(false);	// Unlock command menu if it was locked
	}

	void Update()
	{
		if (!gameObject.activeSelf) return;		// Do nothing if menu is not active

		if (owner.currentState == BattleUIState.SkillSelection) 
		{
			if (Input.GetKeyDown(KeyCode.W))
			{
				// Move selection up in the skill list (if not at the top)
				if (index > 0)
					index--;
				RefreshHighlight();
				RefreshDescription();
			}

			if (Input.GetKeyDown(KeyCode.S))
			{
				// Move selection down in the skill list (if not at the bottom)
				if (index < entries.Count - 1)
					index++;
				RefreshHighlight();
				RefreshDescription();
			}

			if (Input.GetKeyDown(KeyCode.Return))
			{
				// Confirm the selected skill
				Confirm();
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				// Cancel and go back to command menu
				CloseMenu();
				owner.currentState = BattleUIState.CommandSelection; // Change state back to command selection
				commandMenu.SetLocked(false);   // Unlock command menu
				commandMenu.RefreshHighlight(); // Refresh highlight in command menu
			}
		}
	}

	void RefreshHighlight()
	{
		// Update the visual highlight of the selected skill entry
		for (int i = 0; i < entries.Count; i++)
		{
			// Change panel color to indicate selection
			var img = entries[i].panel.GetComponent<Image>();

			if (img != null)
				// Highlight selected entry in yellow, others in white
				img.color = (i == index) ? Color.yellow : Color.white;
		}
	}

	void RefreshDescription()
	{
		// Update the skill description panel based on the selected skill
		if (actorEntity != null && index < actorEntity.skills.Count)
		{
			// Get the selected skill data
			var skill = actorEntity.skills[index];

			// Populate description UI elements
			if (descriptionName != null) descriptionName.text = skill.displayName;
			if (descriptionType != null) descriptionType.text = skill.type;
			if (descriptionText != null) descriptionText.text = skill.description;
		}
	}

	void Confirm()
	{
		// Confirm the selected skill and proceed with action
		if (actorEntity == null || index >= actorEntity.skills.Count) return;

		var selected = actorEntity.skills[index].id;	// Get selected skill ID
		Debug.Log("Skill Selected: " + selected);

		if (commandMenu != null)
		{
			// Ensure command menu is unlocked and reset command menu's selection index 
			commandMenu.SetLocked(false);
			commandMenu.index = 0;
			commandMenu.lastSelectedIndex = 0;
			commandMenu.RefreshHighlight();
		}

		// Add action here:

		CloseMenu();		// Close skill menu
		owner.BackToPartySelection();	// Return to party selection
	}
}