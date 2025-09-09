using UnityEngine;

// This script manages the party panel UI, displaying multiple character slots.
public class PartyPanelUI : MonoBehaviour
{
	public CharacterSlotUI[] slots;			// Array of character slots in the party panel.
	public BattleActorEntity[] actors;		// Array of BattleActorEntity representing party members.

	void Start()
	{
		// Initialize the character slots with the corresponding actors.
		for (int i = 0; i < slots.Length; i++)
		{
			// Assign actor to slot if available
			if (i < actors.Length)
			{
				slots[i].SetActor(actors[i]);
			}
		}
	}
}
