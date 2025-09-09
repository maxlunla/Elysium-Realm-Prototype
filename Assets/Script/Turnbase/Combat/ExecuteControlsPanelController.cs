using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ExecuteEntry
{
	public string id;			// "Confirm", "Cancel"
	public Button button;		// Button component
}

public class ExecuteControlsPanelController : MonoBehaviour
{
	public ExecuteEntry[] entries;     // Array of entries ("Confirm", "Cancel")
	private int index = 0;             // Current selected index

	// Colors for highlighting buttons and normal state
	public Color highlightColor = Color.yellow;
	private Color normalColor = Color.white;

	// Called when the panel is enabled
	void OnEnable()
	{
		index = 0;
		RefreshHighlight();
	}

	void Update()
	{
		if (!gameObject.activeSelf) return;		// Do nothing if panel is not active

		// Only allow input if the current state is Confirming
		if (BattleManager.Instance.partySelector.currentState != BattleUIState.Confirming) return;

		if (Input.GetKeyDown(KeyCode.W))
		{
			// Move selection up
			int newIndex = index - 1;
			if (newIndex >= 0) index = newIndex;
			RefreshHighlight();
		}

		if (Input.GetKeyDown(KeyCode.S))
		{
			// Move selection down
			int newIndex = index + 1;
			if (newIndex < entries.Length) index = newIndex; // ไม่วน
			RefreshHighlight();
		}

		if (Input.GetKeyDown(KeyCode.Return))
		{
			// Confirm selection
			Confirm();
		}
	}

	// Refresh button highlights based on current selection
	void RefreshHighlight()
	{
		// Update button colors
		for (int i = 0; i < entries.Length; i++)
		{
			var img = entries[i].button?.GetComponent<Image>();
			if (img != null)
				img.color = (i == index) ? highlightColor : normalColor;
		}

		// Set the selected GameObject in the EventSystem for controller/keyboard navigation
		if (entries.Length > 0 && entries[index].button != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(entries[index].button.gameObject);
		}
	}

	// Confirm the current selection
	void Confirm()
	{
		var selected = entries[index].id;		// Get the ID of the selected entry

		// Handle the selected action ("Confirm" or "Cancel")
		if (selected == "Execute")
		{
			// Execute all queued actions and proceed with the battle
			Debug.Log("Execute queued actions!");

			BattleManager.Instance.partySelector.currentState = BattleUIState.Executing;
			// TODO: Implement action execution logic here
			StartCoroutine(BattleManager.Instance.executeController.ExecuteQueuedActions());
		}
		else if (selected == "Cancel")
		{
			// Clear all queued actions and return to party selection
			BattleManager.Instance.ClearAllQueuedActions();
			BattleManager.Instance.partySelector.currentState = BattleUIState.PartySelection;

			BattleManager.Instance.partySelector.inputLocked = true;
			BattleManager.Instance.partySelector.BackToPartySelection();

			gameObject.SetActive(false);
		}
	}
}