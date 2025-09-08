using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

// This script manages all battle actors in the scene and categorizes them into players and enemies
public class BattleManager : MonoBehaviour
{
	public static BattleManager Instance;		// Singleton instance of BattleManager, attached to BattleManager GameObject

	public List<BattleActorEntity> enemies = new List<BattleActorEntity>();		// List of enemy actors in the scene
	public List<BattleActorEntity> players = new List<BattleActorEntity>();     // List of player actors in the scene																// List เก็บ queue
	public ActionQueuePanel actionQueuePanel;

	[Header("References")]
	public PartySelector partySelector;
	public SkillMenuController skillMenuController;

	[Header("AP/Action Point")]
	private int maxAP = 15;					// Maximum Action Points (AP) a character can have
	public int currentAP = 15;
	private int maxAction = 5;
	public int currentAction = 5;

	private List<ActionQueueItem> queuedActions = new List<ActionQueueItem>();

	public class ActionQueueItem
	{
		public BattleActorEntity actor;
		public SkillData skill;
		public ActorCommand action;
		public BattleActorEntity target;
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
		BattleActorEntity[] allActors = FindObjectsOfType<BattleActorEntity>();

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
		UpdateAPActionUI();
	}

	private void UpdateAPActionUI()
	{
		if (actionQueuePanel == null) return;

		actionQueuePanel.tpText.text = "TP: " + currentAP;
		actionQueuePanel.actionText.text = "Action: " + currentAction;

		for (int i = 0; i < actionQueuePanel.actionIcons.Length; i++)
		{
			actionQueuePanel.actionIcons[i].color = i < queuedActions.Count ? Color.white : Color.black;
		}
	}

	public void AddToActionQueue(BattleActorEntity actor, SkillData skill, ActorCommand action, BattleActorEntity target)
	{
		if (currentAP <= 0 || currentAction <= 0) return;

		// คำนวณค่า AP ของ Action/Skill
		int tpCost = 1; // default
		if (skill != null)
			tpCost = skill.tpCost; // สมมติ SkillData มี property tpCost
		else if (action != null)
			tpCost = action.tpCost; // สมมติ ActorCommand มี property tpCost

		// เช็คว่าพอมี AP ไหม
		if (tpCost > currentAP) return;

		var item = new ActionQueueItem
		{
			actor = actor,
			skill = skill,
			action = action,
			target = target
		};

		queuedActions.Add(item);

		// ลดค่า AP และ Action
		currentAP -= tpCost;
		currentAction--;

		UpdateAPActionUI();
	}

	// อัพเดท UI
	void UpdateActionQueueUI()
	{
		if (BattleManager.Instance.actionQueuePanel == null) return;

		var icons = BattleManager.Instance.actionQueuePanel.actionIcons;

		for (int i = 0; i < icons.Length; i++)
		{
			if (i < queuedActions.Count)
				icons[i].color = Color.white;  // slot มี Action/Skill
			else
				icons[i].color = Color.black;  // slot ว่าง
		}
	}
}