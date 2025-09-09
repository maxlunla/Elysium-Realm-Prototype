using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// This script manages the UI for a character slot in the party panel.
public class CharacterSlotUI : MonoBehaviour
{
	[Header("UI References")]
	public TMP_Text nameText;

	[Header("HP UI")]
	public Image hpFill;
	public TMP_Text hpText;
	public TMP_Text maxhpText;

	[Header("MP UI")]
	public Image mpFill;
	public TMP_Text mpText;
	public TMP_Text maxmpText;

	[Header("States UI")]
	public Transform statesContainer;   // State icons container
	public GameObject stateIconPrefab;  // State icon prefab

	[Header("States UI")]
	public Image[] stateIcons;			// State icons array (10 icons)

	private BattleActorEntity actor;	// Reference to the BattleActorEntity

	void Update()
	{
		// Update UI only if actor is assigned
		if (actor != null)
		{
			// Name of the character
			nameText.text = actor.characterName;

			// HP of the character
			hpFill.fillAmount = (float)actor.currentHP / actor.maxHP;
			hpText.text = $"{actor.currentHP}";
			maxhpText.text = $"/{actor.maxHP}";

			// MP of the character
			mpFill.fillAmount = (float)actor.currentMP / actor.maxMP;
			mpText.text = $"{actor.currentMP}";
			maxmpText.text = $"/{actor.maxMP}";

			// States of the character
			UpdateStatesUI();
		}
	}

	public void SetActor(BattleActorEntity newActor)
	{
		// Assign the actor to this slot, called from PartyPanelUI.
		actor = newActor;
	}

	private void UpdateStatesUI()
	{
		// Update the state icons based on the actor's current states.
		// Close all state icons first
		for (int i = 0; i < stateIcons.Length; i++)
		{
			stateIcons[i].gameObject.SetActive(false);
		}

		// Then enable and set icons for current states
		for (int i = 0; i < actor.states.Count && i < stateIcons.Length; i++)
		{
			stateIcons[i].sprite = actor.states[i].icon;
			stateIcons[i].gameObject.SetActive(true);
		}
	}
}
