using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using static BattleActorEntity;

// Data structure for an action command
[System.Serializable]
public class ActorCommand
{
	public string id;				// "Attack", "Guard", "Skills", "Items", "Escape"
	public string displayName;		// Name of action to show in UI
	public bool available = true;   // True if action can be selected
	public int mpCost;              // Mana cost
	public int tpCost;              // Technical points cost
	public enum TargetType
	{
		Single_Enemy,
		Single_Ally,
		Multiple_Enemies,
		Multiple_Allies,
		Self,
	}
	public TargetType targetType;
}

// Data structure for a skill
[System.Serializable]
public class SkillData
{
	public string id;				// Unique identifier for the skill
	public string displayName;		// Name of skill to show in UI
	public string description;		// Description of the skill
	public int mpCost;				// Mana cost
	public int tpCost;				// Technical points cost
	public int cooldown;			// Cooldown in turns
	public string type;             // "Physical", "Magical", "Attack", "Buff"
	public bool isCooldown = false;			// True if skill is on cooldown

	public enum TargetType
	{
		Single_Enemy,
		Single_Ally,
		Multiple_Enemies,
		Multiple_Allies,
		Self,
	}
	public TargetType targetType;
}

// Data structure for a state (Buff/Debuff)
[System.Serializable]
public class StateData
{
	public string stateName;		// Name of the state
	public Sprite icon;				// Icon representing the state
	public int duration;			// Duration in turns
	public string description;		// Description of the state effect
}

// This script represents a battle actor entity with health, mana, and states.
public class BattleActorEntity : MonoBehaviour
{
	public string characterName = "Player";

	// Type of actor: Player, Enemy, Boss
	public enum ActorType
	{
		Player,
		Enemy,
		Boss
	}
	public ActorType playerType;

	[Header("HP and MP")]
	public int maxHP = 100;
	public int currentHP = 100;
	public int maxMP = 50;
	public int currentMP = 50;

	[Header("Stats")]
	public int ATK = 50;
	public int DEF = 50;
	public int MAT = 50;
	public int MDF = 50;
	public int AGI = 50;
	public int LUK = 50;

	public List<ActorCommand> commands = new List<ActorCommand>();		// List of available action commands
	public List<SkillData> skills = new List<SkillData>();				// List of skills the actor can use
	public List<StateData> states = new List<StateData>();              // List of current states (buffs/debuffs)

	[Header("Enemy UI (On Head)")]
	public Canvas hpCanvas;   // Canvas บนหัว
	public GameObject cursor; // ถ้าจะใช้ cursor ของแต่ละตัว (optional)

	public void ShowCursor(bool show)
	{
		if (cursor != null)
			cursor.SetActive(show);

		if (hpCanvas != null)
			hpCanvas.gameObject.SetActive(show);
	}

	public void TakeDamage(int amount)
	{
		// Reduce current HP but not below zero.
		currentHP = Mathf.Max(0, currentHP - amount);
	}

	public void UseMP(int amount)
	{
		// Reduce current MP but not below zero.
		currentMP = Mathf.Max(0, currentMP - amount);
	}

	public void AddState(StateData newState)
	{
		// Add a new state to the list.
		states.Add(newState);
	}

	public void RemoveState(StateData state)
	{
		// Remove a state from the list.
		states.Remove(state);
	}

	public void TickStates()
	{
		// Decrease duration of each state and remove if duration is zero.
		// Call when a turn ends.
		for (int i = states.Count - 1; i >= 0; i--)
		{
			states[i].duration--;

			// Remove state if duration has expired.
			if (states[i].duration <= 0)
			{
				RemoveState(states[i]);
			}
		}
	}
}
