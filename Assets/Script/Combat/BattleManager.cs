using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

// This script manages all battle actors in the scene and categorizes them into players and enemies
public class BattleManager : MonoBehaviour
{
	public static BattleManager Instance;				// Singleton instance of BattleManager, attached to BattleManager GameObject

	public List<BattleActorEntity> enemies = new List<BattleActorEntity>();		// List of enemy actors in the scene
	public List<BattleActorEntity> players = new List<BattleActorEntity>();     // List of player actors in the scene																// List เก็บ queue
	public ActionQueuePanel actionQueuePanel;

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
		public BattleActorEntity target;				// The target of the action
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
		UpdateAPActionUI();			// Update AP/Action UI each frame
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
		if (currentAP <= 0 || currentAction <= 0) return;		// Check if there are enough AP and Actions left

		// Calculate the AP cost of the skill or action
		int apCost = 1;					// Default AP cost is 1 at minimum
		if (skill != null)
			apCost = skill.apCost;		// If using a skill, get its AP cost
		else if (action != null)
			apCost = action.apCost;		// If using an action, get its AP cost

		if (apCost > currentAP) return;	// Check if there are enough AP to perform the action

		// Create a new ActionQueueItem and add it to the queue
		var item = new ActionQueueItem
		{
			actor = actor,
			skill = skill,
			action = action,
			target = target
		};

		queuedActions.Add(item);		// Add the action to the queue

		// Reduce current AP and Action count based on the action taken
		currentAP -= apCost;
		currentAction--;

		UpdateAPActionUI();				// Update AP and Action in the UI
	}

	// UpdateActionQueueUI to refresh the icon colors based on queued actions in the queue
	void UpdateActionQueueUI()
	{
		if (BattleManager.Instance.actionQueuePanel == null) return;

		var icons = BattleManager.Instance.actionQueuePanel.actionIcons;		// Get action icons from the panel reference 

		// Loop through each icon and set its color based on whether there is a queued action or not
		for (int i = 0; i < icons.Length; i++)
		{
			if (i < queuedActions.Count)
				icons[i].color = Color.white;  // White color for filled slot
			else
				icons[i].color = Color.black;  // black color for empty slot
		}
	}
}