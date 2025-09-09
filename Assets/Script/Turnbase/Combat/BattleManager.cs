using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

// This script manages all battle actors in the scene and categorizes them into players and enemies
public class BattleManager : MonoBehaviour
{
	public static BattleManager Instance;				// Singleton instance of BattleManager, attached to BattleManager GameObject

	public List<BattleActorEntity> enemies = new List<BattleActorEntity>();		// List of enemy actors in the scene
	public List<BattleActorEntity> players = new List<BattleActorEntity>();     // List of player actors in the scene																// List เก็บ queue
	public ActionQueuePanel actionQueuePanel;
	public ExecuteController executeController;

	[Header("References")]
	public PartySelector partySelector;					// Reference to PartySelector component
	public SkillMenuController skillMenuController;		// Reference to SkillMenuController component

	[Header("AP/Action Point")]
	private int maxAP = 15;								// Maximum Action Points (AP), all character share the same AP pool (default 15)
	public int currentAP = 15;							// Current Action Points (AP) a character has
	private int maxAction = 5;							// Maximum number of actions, all character share the same action pool (default 5)
	public int currentAction = 5;						// Current number of actions a character can take

	private List<ActionQueueItem> queuedActions = new List<ActionQueueItem>();	// List to store queued actions

	// ActionQueueItem for storing action details in the queue list (Actor, Skill, Action, Target)
	public class ActionQueueItem
	{
		public BattleActorEntity actor;					// The actor performing the action
		public SkillData skill;							// The skill being used
		public ActorCommand action;						// The action being performed
		public BattleActorEntity target;                // The target of the action
		public int mpCost;								// MP cost at the moment of queuing
		public int apCost;								// AP cost at the moment of queuing
	}

	private void Awake()
	{
		// Singleton pattern to ensure only one instance exists
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		// Find all BattleActorEntity instances in the scene
		BattleActorEntity[] allActors = Object.FindObjectsByType<BattleActorEntity>(FindObjectsSortMode.None);

		// Clear existing lists
		enemies.Clear();
		players.Clear();

		// Categorize actors into players and enemies
		foreach (var actor in allActors)
		{
			switch (actor.playerType)
			{
				case BattleActorEntity.ActorType.Player:		// Player type
					players.Add(actor);
					break;
				case BattleActorEntity.ActorType.Enemy:			// Enemy type (regular)
				case BattleActorEntity.ActorType.Boss:			// Enemy type (boss)
					enemies.Add(actor);
					break;
			}
		}

		// Sort enemies based on their sibling index in the hierarchy
		enemies = enemies.OrderBy(e => e.transform.GetSiblingIndex()).ToList();
	}

	private void Update()
	{
		UpdateAPActionUI();		// Update AP/Action UI each frame

		if (Input.GetKeyDown(KeyCode.P))	// Press 'P' to debug queued actions
		{
			DebugQueuedActions();
		}

		// If not in confirming state, allow undo and clear actions
		if (Input.GetKeyDown(KeyCode.Q) && (BattleManager.Instance.partySelector.currentState != BattleUIState.Confirming))
		{
			UndoLastQueuedAction();			// Press 'Q' to undo the last queued action
		}

		// If not in confirming state, allow clearing all actions
		if (Input.GetKeyDown(KeyCode.R) && (BattleManager.Instance.partySelector.currentState != BattleUIState.Confirming))
		{
			ClearAllQueuedActions();		// Press 'R' to clear all queued actions
		}
	}

	// Update AP and Action UI elements, called when AP or Action changes
	private void UpdateAPActionUI()
	{
		if (actionQueuePanel == null) return;

		// Update AP and Action text in the UI
		actionQueuePanel.apText.text = "AP: " + currentAP;
		actionQueuePanel.actionText.text = "Action: " + currentAction;

		// Update action icons based on the number of queued actions
		for (int i = 0; i < actionQueuePanel.actionIcons.Length; i++)
		{
			// Set icon color to white if there is a queued action, otherwise black
			actionQueuePanel.actionIcons[i].color = i < queuedActions.Count ? Color.white : Color.black;
		}
	}

	// Called by TargetSelector when a target is selected to add the action to the queue
	public void AddToActionQueue(BattleActorEntity actor, SkillData skill, ActorCommand action, BattleActorEntity target)
	{
		if (currentAP <= 0 || currentAction <= 0) return;       // Check if there are enough AP and Actions left

		// Calculate the AP cost of the skill or action
		int apCost = skill != null ? skill.apCost : (action != null ? action.apCost : 1);
		int mpCost = skill != null ? skill.mpCost : (action != null ? action.mpCost : 0);

		if (apCost > currentAP) return;	// Check if there are enough AP to perform the action

		// Create a new ActionQueueItem and add it to the queue
		var item = new ActionQueueItem
		{
			actor = actor,
			skill = skill,
			action = action,
			target = target,
			mpCost = skill != null ? skill.mpCost : (action != null ? action.mpCost : 0),
			apCost = skill != null ? skill.apCost : (action != null ? action.apCost : 1)
		};

		queuedActions.Add(item);		// Add the action to the queue

		// Reduce current AP and Action count based on the action taken
		currentAP -= apCost;
		currentAction--;

		// **ลด MP ของตัวละครทันที**
		if (mpCost > 0)
		{
			actor.UseMP(mpCost);
		}

		UpdateAPActionUI();             // Update AP and Action in the UI
		UpdateActionQueueUI();			// Update action queue UI
	}

	// UpdateActionQueueUI to refresh the icon colors based on queued actions in the queue
	void UpdateActionQueueUI()
	{
		if (BattleManager.Instance.actionQueuePanel == null) return;

		var icons = BattleManager.Instance.actionQueuePanel.actionIcons;        // Get action icons from the panel reference 

		// Loop through each icon and set its color based on whether there is a queued action or not
		for (int i = 0; i < actionQueuePanel.actionIcons.Length; i++)
		{
			var icon = actionQueuePanel.actionIcons[i];

			if (icon == null) continue;

			if (i < queuedActions.Count)
			{
				// White color for queued actions
				icon.color = Color.white;
			}
			else
			{
				// Black color for empty slots
				icon.color = Color.black;
			}
		}
	}

	public void UndoLastQueuedAction()
	{
		if (queuedActions.Count == 0) return; // Do nothing if there are no queued actions

		// Remove the last action from the queue
		var lastItem = queuedActions[queuedActions.Count - 1];
		queuedActions.RemoveAt(queuedActions.Count - 1);

		// Restore AP and Action count
		currentAP += lastItem.apCost;
		currentAction++;

		// Restore MP if the action used MP
		if (lastItem.mpCost > 0 && lastItem.actor != null)
		{
			lastItem.actor.RestoreMP(lastItem.mpCost); // สมมติว่ามีฟังก์ชัน RestoreMP ใน BattleActorEntity
		}

		UpdateAPActionUI();		// Update AP/Action UI
		UpdateActionQueueUI();	// Update action queue UI
	}

	public void ClearAllQueuedActions()
	{
		if (queuedActions.Count == 0) return;

		// Restore AP, Action, and MP for all queued actions
		foreach (var item in queuedActions)
		{
			currentAP += item.apCost;
			currentAction++;

			if (item.mpCost > 0 && item.actor != null)
			{
				item.actor.RestoreMP(item.mpCost);
			}
		}

		// Clear the action queue
		queuedActions.Clear();

		UpdateAPActionUI();     // Update AP/Action UI
		UpdateActionQueueUI();  // Update action queue UI
	}

	public List<ActionQueueItem> GetQueuedActions()
	{
		return queuedActions; // คืน queue ปัจจุบัน
	}

	public void DebugQueuedActions()
	{
		Debug.Log("=== Queued Actions ===");
		for (int i = 0; i < queuedActions.Count; i++)
		{
			var item = queuedActions[i];
			string skillName = item.skill != null ? item.skill.displayName : "None";
			string actionName = item.action != null ? item.action.displayName : "None";
			string targetName = item.target != null ? item.target.characterName : "None";

			Debug.Log(
				$"[{i}] Actor: {item.actor.characterName}, Skill: {skillName}, Action: {actionName}, Target: {targetName}, AP: {item.apCost}, MP: {item.mpCost}"
			);
		}
		Debug.Log("=====================");
	}
}