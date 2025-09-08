using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using static BattleActorEntity;

// This script defines the data structures and behavior for battle actors in a turn-based combat system

// Data structure for an action command (e.g., Attack, Guard, Skill, Item)
[System.Serializable]
public class ActorCommand
{
	public string id;				// "Attack", "Guard", "Skills", "Items", "Escape"
	public string displayName;		// Name of action to show in UI
	public bool available = true;   // True if action can be selected
	public int mpCost;              // Mana cost
	public int apCost;              // Technical points cost
	public enum TargetType			// Type of targeting for the action
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
	public int apCost;				// Technical points cost
	public int cooldown;			// Cooldown in turns
	public string type;             // "Physical", "Magical", "Attack", "Buff"
	public bool isCooldown = false;	// True if skill is on cooldown

	public enum TargetType			// Type of targeting for the skill
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

public class BattleActorEntity : MonoBehaviour
{
	public string characterName = "Player";		// Name of the character

	public enum ActorType						// Type of actor in battle (affects targeting)
	{
		Player,
		Enemy,
		Boss
	}
	public ActorType playerType;

	[Header("HP and MP")]
	public int maxHP = 100;						// Maximum health points
	public int currentHP = 100;					// Current health points
	public int maxMP = 50;						// Maximum mana points
	public int currentMP = 50;					// Current mana points

	[Header("Stats")]
	public int ATK = 50;						// Attack power 
	public int DEF = 50;						// Defense power
	public int MAT = 50;						// Magic attack power
	public int MDF = 50;						// Magic defense power
	public int AGI = 50;						// Agility (reduce skill cooldown and AP cost)
	public int LUK = 50;						// Luck (affects critical hits, evasion, etc.)

	public List<ActorCommand> commands = new List<ActorCommand>();		// List of available action commands
	public List<SkillData> skills = new List<SkillData>();				// List of skills the actor can use
	public List<StateData> states = new List<StateData>();              // List of current states (buffs/debuffs)

	[Header("Enemy UI (On Head)")]
	public Canvas hpCanvas;			// HP UI for enemies
	public Canvas BosshpCanvas;		// HP UI for bosses
	public GameObject cursor;       // Cursor object to show/hide

	// ShowCursor for control the cursor visibility and color (used in target selection)
	public void ShowCursor(bool value, Color? color = null)
	{
		if (cursor != null)
		{
			cursor.SetActive(value);

			// Change cursor color if a color is provided
			if (value && color.HasValue)
			{
				var meshRenderer = cursor.GetComponent<MeshRenderer>();
				if (meshRenderer != null)
				{
					meshRenderer.material.color = color.Value;
				}
			}
		}
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

	public void RestoreMP(int amount)
	{
		currentMP += amount;
		if (currentMP > maxMP) currentMP = maxMP;
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
