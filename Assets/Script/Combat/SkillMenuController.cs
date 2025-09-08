using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// This script manages the skill selection menu during battle. It allows players to choose skills for their characters.

// Data structure for each skill in the menu. Holds references to UI elements.
[System.Serializable]
public class SkillEntry
{
	public GameObject panel;					// Panel GameObject for the skill entry (used for highlighting)
	public TextMeshProUGUI nameText;			// Skill name text
	public TextMeshProUGUI apText;				// AP cost text
	public TextMeshProUGUI mpText;				// MP cost text
	public TextMeshProUGUI cooldownText;		// Cooldown text
}

public class SkillMenuController : MonoBehaviour
{
	[Header("Skill List (5)")]
	public List<SkillEntry> entries;			// List of skill in the menu. (max 5 skills)

	[Header("Description UI")]
	public TextMeshProUGUI descriptionName;		// Skill name in description panel
	public TextMeshProUGUI descriptionType;		// Skill type in description panel
	public TextMeshProUGUI descriptionText;		// Skill description in description panel

	private int index = 0;						// Current selected skill index
	private PartySelector owner;				// Owner party selector
	private CommandMenuController commandMenu;	// Reference to command menu controller
	private BattleActorEntity actorEntity;		// Current actor entity

	public GameObject descriptionPanel;			// Panel for showing skill description
	public bool skillLocked = false;			// Lock input when processing to avoid multiple inputs.

	// Called from CommandMenuController to open the skill menu for a specific actor.
	public void OpenMenu(PartySelector ownerSelector, CommandMenuController cmdMenu, BattleActorEntity actorEntity)
	{
		owner = ownerSelector;				// Set owner
		commandMenu = cmdMenu;				// Set command menu reference
		this.actorEntity = actorEntity;		// Set current actor entity

		gameObject.SetActive(true);			// Show skill menu

		if (descriptionPanel != null) descriptionPanel.SetActive(true);		// Show description panel
		index = 0;							// Reset index

		// Populate skill entries based on actor's skills from BattleActorEntity
		for (int i = 0; i < entries.Count; i++)
		{
			var entry = entries[i];		// Get skill entry

			// Set text for each skill if available
			if (i < actorEntity.skills.Count)
			{
				var skill = actorEntity.skills[i];						// Get skill data from actor entity
				entry.nameText.text = skill.displayName;				// Set skill name
				entry.apText.text = skill.apCost.ToString() + "AP";		// Set AP cost
				entry.mpText.text = skill.mpCost.ToString() + "MP";		// Set MP cost
				entry.cooldownText.text = skill.cooldown > 0 ? skill.cooldown + (skill.cooldown == 1 ? " Turn" : " Turns") : "";	// Set cooldown text

				// Change color based on whether the skill can be used with current AP
				if (skill.apCost > BattleManager.Instance.currentAP)
				{
					// Gray out the skill if not enough AP
					entry.panel.GetComponent<Image>().color = Color.gray;
				}
				else
				{
					// White color if skill is usable
					entry.panel.GetComponent<Image>().color = Color.white;
				}
			}
			else
			{
				entry.panel.SetActive(false);		// Hide unused skill if actor has less than 5 skills
			}
		}

		RefreshHighlight();		// Highlight the first skill by default
		RefreshDescription();	// Show description of the first skill selected
	}

	// lockUnput is called to lock or unlock input processing of the skill menu.
	public void lockedInput(bool value)
	{
		skillLocked = value;
	}

	// CloseMenu is called to close the skill menu.
	public void CloseMenu()
	{
		gameObject.SetActive(false);			// Hide skill menu

		// Hide description panel and unlock command menu
		if (descriptionPanel != null)
			descriptionPanel.SetActive(false);	// Hide description panel
		if (commandMenu != null)
			commandMenu.SetLocked(true);		// Set true for locking command menu input when skill menu is closed (to avoid input overlap)
	}

	void Update()
	{
		if (!gameObject.activeSelf) return;		// Do nothing if menu is not active

		// Ignore input if skillLocked is true (to avoid multiple inputs during processing)
		if (skillLocked)
		{
			skillLocked = false;
			return;		// Skip input processing this frame
		}

		// If in skill selection state, process input for navigating and selecting skills
		if (owner.currentState == BattleUIState.SkillSelection) 
		{
			int skillCount = actorEntity.skills.Count;		// Get number of skills the actor has

			if (Input.GetKeyDown(KeyCode.W))
			{
				// Move selection up in the skill list
				int newIndex = index - 1;

				// Skip skills that cannot be used due to insufficient AP
				while (newIndex >= 0)
				{
					if (newIndex < actorEntity.skills.Count &&
						actorEntity.skills[newIndex].apCost <= BattleManager.Instance.currentAP)
					{
						index = newIndex;
						break;
					}
					newIndex--;
				}

				RefreshHighlight();		// Update highlight to new selection
				RefreshDescription();	// Update description to new selection
			}

			if (Input.GetKeyDown(KeyCode.S))
			{
				// Move selection down in the skill list
				int newIndex = index + 1;

				// Skip skills that cannot be used due to insufficient AP
				while (newIndex < actorEntity.skills.Count)
				{
					if (actorEntity.skills[newIndex].apCost <= BattleManager.Instance.currentAP)
					{
						index = newIndex;
						break;
					}
					newIndex++;
				}

				RefreshHighlight();		// Update highlight to new selection
				RefreshDescription();	// Update description to new selection
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
				commandMenu.SetLocked(true);		// Lock command menu input briefly to avoid overlap
				commandMenu.RefreshHighlight();		// Refresh highlight in command menu to show current selection
			}
		}
	}

	// RefreshHighlight updates the visual highlight of the currently selected skill.
	void RefreshHighlight()
	{
		int currentAP = BattleManager.Instance.currentAP;		// Get current AP from BattleManager

		// Check each skill entry to update its highlight and color based on selection and AP cost
		for (int i = 0; i < entries.Count; i++)
		{
			var img = entries[i].panel.GetComponent<Image>();	// Get the Image component of the skill panel
			if (img == null) continue;			// Skip if no Image component found

			// If not enough AP to use the skill, gray it out
			if (i < actorEntity.skills.Count && actorEntity.skills[i].apCost > currentAP)
			{
				img.color = Color.gray;
			}
			else
			{
				// If this skill is the currently selected one, highlight it in yellow; otherwise, white
				img.color = (i == index) ? Color.yellow : Color.white;
			}
		}
	}

	// RefreshDescription updates the skill description panel based on the currently selected skill.
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

	// Confirm processes the selected skill and initiates the corresponding action.
	void Confirm()
	{
		// Confirm the selected skill and proceed with action
		if (actorEntity == null || index >= actorEntity.skills.Count) return;

		var skillData = actorEntity.skills[index];		// Get the selected skill data

		// If not enough AP to use the skill, do nothing
		if (skillData.apCost > BattleManager.Instance.currentAP)
		{
			Debug.Log("Not enough AP to use this skill!");
			return;
		}

		// Lock the command menu and reset its selection index to avoid input overlap during targeting phase
		if (commandMenu != null)
		{
			commandMenu.SetLocked(true);		// Lock command menu input
			commandMenu.index = 0;				// Reset in index to first command
			commandMenu.lastSelectedIndex = 0;	// Reset last selected index as well
			commandMenu.RefreshHighlight();		// Refresh highlight in command menu
		}

		// Change state to TargetSelection to proceed with targeting phase
		FindAnyObjectByType<TargetSelector>().StartTargeting(actorEntity, skillData);
	}

	// HideSkillDescriptionPanel hides the skill description panel.
	public void HideSkillDescriptionPanel()
	{
		// Hide the skill description panel if it exists
		if (descriptionPanel != null)
			descriptionPanel.SetActive(false);
	}
}