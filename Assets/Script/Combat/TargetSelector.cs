using System.Collections.Generic;
using UnityEngine;

// This script manages the targeting system during combat, allowing players to select targets for actions and skills.
public class TargetSelector : MonoBehaviour
{
	public int currentIndex = 0;						// Index of the currently selected target

	private List<BattleActorEntity> currentTargets;		// List of potential targets based on the selected action or skill 
	private SkillData currentSkill;						// Currently selected skill
	private ActorCommand currentAction;					// Currently selected action
	private BattleActorEntity currentActor;				// The actor who is currently selecting a target

	public CommandMenuController commandMenuController;	// Reference to CommandMenuController component
	public SkillMenuController skillMenuController;		// Reference to SkillMenuController component

	public GameObject playerCursor;						// Cursor GameObject from different function

	[Header("Cursor Colors")]
	public Color enemyCursorColor = Color.red;			// Color for enemy cursor
	public Color allyCursorColor = Color.cyan;			// Color for ally cursor
	public Color selfCursorColor = Color.green;			// Color for self cursor

	// StartTargeting called by other script to initiate the targeting process with the selected actor, skill, or action
	public void StartTargeting(BattleActorEntity actor, SkillData skill = null, ActorCommand action = null)
	{
		currentActor = actor;		// Get the actor who is selecting the target
		currentSkill = skill;		// Get the selected skill
		currentAction = action;		// Get the selected action

		currentTargets = new List<BattleActorEntity>();	// Initialize the target list
		currentIndex = 0;			// Reset the current index

		playerCursor.gameObject.SetActive(false);		// Disable the cursor on other function

		if (skill != null)
		{
			// Set the target list based on the skill's target type
			switch (skill.targetType)
			{
				case SkillData.TargetType.Self:
					// Self-targeting skill
					currentTargets.Add(actor);
					break;
				case SkillData.TargetType.Single_Enemy:
				case SkillData.TargetType.Multiple_Enemies:
					// Single or multiple enemy targeting skill
					currentTargets = BattleManager.Instance.enemies;
					break;
				case SkillData.TargetType.Single_Ally:
				case SkillData.TargetType.Multiple_Allies:
					// Single or multiple ally targeting skill
					currentTargets = BattleManager.Instance.players;
					break;
			}
		}
		else if (action != null)
		{
			// Set the target list based on the action's target type
			switch (action.targetType)
			{
				case ActorCommand.TargetType.Self:
					currentTargets.Add(actor);
					break;
				case ActorCommand.TargetType.Single_Enemy:
					currentTargets = BattleManager.Instance.enemies;
					break;
			}
		}

		RefreshCursor();		// Refresh the cursor to show the current target
		BattleManager.Instance.partySelector.currentState = BattleUIState.Targeting;	// Set the state to Targeting
	}

	void Update()
	{
		// Only process input if in Targeting state
		if (BattleManager.Instance.partySelector.currentState != BattleUIState.Targeting) return;

		// If there are no targets, exit the function
		if (currentTargets == null || currentTargets.Count == 0) return;

		bool canMove = false;		// Flag to check if the cursor can move or not (based on target type)

		// Check if the current skill or action allows cursor movement (single target only)
		if (currentSkill != null)
			canMove = currentSkill.targetType == SkillData.TargetType.Single_Enemy ||
					  currentSkill.targetType == SkillData.TargetType.Single_Ally;
		else if (currentAction != null)
			canMove = currentAction.targetType == ActorCommand.TargetType.Single_Enemy;		// Only Attack action allows cursor movement

		// If the cursor can move, process A/D input to change the current index
		if (canMove)
		{
			if (Input.GetKeyDown(KeyCode.A) && currentIndex > 0)
			{
				// Move cursor left if not at the first target
				currentIndex--;
				RefreshCursor();
			}
			if (Input.GetKeyDown(KeyCode.D) && currentIndex < currentTargets.Count - 1)
			{
				// Move cursor right if not at the last target
				currentIndex++;
				RefreshCursor();
			}
		}

		if (Input.GetKeyDown(KeyCode.Return))
		{
			// Confirm the target selection
			ConfirmTarget();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// Cancel the target selection
			CancelTargeting();
		}
	}

	// RefreshCursor updates the cursor's position and visibility based on the current target selection
	void RefreshCursor()
	{
		if (currentTargets == null || currentTargets.Count == 0) return;

		bool showAll = false;		// Flag to determine if all targets should show the cursor (for multi-target skills)

		// Check if the current skill is a multi-target skill
		if (currentSkill != null)
			showAll = currentSkill.targetType == SkillData.TargetType.Multiple_Enemies ||
					  currentSkill.targetType == SkillData.TargetType.Multiple_Allies;

		// Hide cursor on all targets first
		foreach (var t in currentTargets)
			t.ShowCursor(false);

		Color cursorColor;		// Determine the cursor color based on target type

		// Set cursor color based on whether the target is self, enemy, or ally
		if (currentSkill != null && currentSkill.targetType == SkillData.TargetType.Self)
		{
			// Self-targeting skill
			cursorColor = selfCursorColor;
		}
		else if (currentAction != null && currentAction.targetType == ActorCommand.TargetType.Self)
		{
			// Self-targeting action
			cursorColor = selfCursorColor;
		}
		else
		{
			// Enemy or ally targeting skill/action
			cursorColor = (currentTargets == BattleManager.Instance.enemies)
				? enemyCursorColor
				: allyCursorColor;
		}

		// Show cursor on the appropriate targets based on whether it's multi-target or single-target
		if (showAll)
		{
			// Show cursor on all targets for multi-target skills or actions
			foreach (var t in currentTargets)
				t.ShowCursor(true, cursorColor);
		}
		else
		{	// Show cursor only on the currently selected target for single-target skills or actions
			if (currentIndex >= 0 && currentIndex < currentTargets.Count)
				currentTargets[currentIndex].ShowCursor(true, cursorColor);
		}
	}

	// ConfirmTarget finalizes the target selection and queues the action
	void ConfirmTarget()
	{
		var target = currentTargets[currentIndex];		// Get the currently selected target

		// Log the action details for debugging
		string actionName = currentSkill != null
			? currentSkill.displayName
			: currentAction?.displayName ?? "Attack";

		int tpCost = currentSkill != null
			? currentSkill.apCost
			: currentAction?.apCost ?? 0;

		int mpCost = currentSkill != null
			? currentSkill.mpCost
			: currentAction?.mpCost ?? 0;

		Debug.Log($"{currentActor.characterName} used {actionName} on {target.characterName} " +
				  $"(TP: {tpCost}, MP: {mpCost})");

		// Hide cursor from all targets
		foreach (var t in currentTargets)
			t.ShowCursor(false);

		// Add the action or skill to the action queue
		BattleManager.Instance.AddToActionQueue(currentActor, currentSkill, currentAction, target);

		// Check if the player has remaining AP or actions to continue
		if (BattleManager.Instance.currentAP <= 0 || BattleManager.Instance.currentAction <= 0)
		{
			// Hide the cursor, close all menu and go to Execute Controls if no AP/actions left
			playerCursor.gameObject.SetActive(false);

			BattleManager.Instance.actionQueuePanel.executeControlsPanel.SetActive(true);		// Show Execute Controls panel
			commandMenuController.CloseMenu();
			skillMenuController.CloseMenu();
			BattleManager.Instance.partySelector.currentState = BattleUIState.Confirming;		// Set state to Confirming
		}
		else
		{
			// If there are remaining AP/actions, go back to Party Selection for next action
			BattleManager.Instance.actionQueuePanel.executeControlsPanel.SetActive(false);
			commandMenuController.SetLocked(true);							// Lock command menu input for block the multiple input
			BattleManager.Instance.partySelector.inputLocked = true;		// Lock party selector input to prevent immediate re-selection
			BattleManager.Instance.partySelector.currentState = BattleUIState.PartySelection;	// Set state to PartySelection
			BattleManager.Instance.partySelector.BackToPartySelection();	// Go back to Party Selection menu
			playerCursor.gameObject.SetActive(true);		// Show the player cursor again
		}

		BattleManager.Instance.skillMenuController.HideSkillDescriptionPanel();			// Hide skill description panel
	}

	// CancelTargeting aborts the target selection and returns to the previous menu
	void CancelTargeting()
	{
		// Hide cursor from all targets
		foreach (var t in currentTargets)
			t.ShowCursor(false);

		BattleManager.Instance.skillMenuController.lockedInput(true);
		BattleManager.Instance.partySelector.inputLocked = true;

		if (currentSkill != null)
		{
			// Back to SkillSelection if targeting from Skill
			BattleManager.Instance.partySelector.currentState = BattleUIState.SkillSelection;
			BattleManager.Instance.partySelector.GoToSkillSelection();
		}
		else
		{
			// Back to CommandSelection if targeting from Command
			BattleManager.Instance.partySelector.currentState = BattleUIState.CommandSelection;
			BattleManager.Instance.partySelector.BackToCommandSelection();
		}

		commandMenuController.SetLocked(true);		// Lock command menu input to block multiple input
		playerCursor.gameObject.SetActive(true);	// Show the player cursor again
	}
}