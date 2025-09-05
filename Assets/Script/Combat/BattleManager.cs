using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This script manages all battle actors in the scene and categorizes them into players and enemies
public class BattleManager : MonoBehaviour
{
	public static BattleManager Instance;		// Singleton instance of BattleManager, attached to BattleManager GameObject

	public List<BattleActorEntity> enemies = new List<BattleActorEntity>();		// List of enemy actors in the scene
	public List<BattleActorEntity> players = new List<BattleActorEntity>();		// List of player actors in the scene

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
}