using UnityEngine;

// This script manages the party selection UI during battle
public class PartySelector : MonoBehaviour
{
	[Header("UI")]
	public CharacterUIHighlight[] partyUI;    // Panels for each party member
	public GameObject commandPanel;           // Action Command UI
	public GameObject skillMenuPanel;         // Skill Menu UI

	[Header("Party")]
	public BattleActor[] party;               // Array of BattleActor in the party

	[Header("Cursor")]
	public SelectionCursor cursor;            // Cursor object to indicate selection

	public BattleUIState currentState = BattleUIState.PartySelection;   // Current state of the battle UI

	public bool inputLocked = false;

	// Get the currently selected BattleActorEntity
	public BattleActorEntity CurrentActor
	{
		get
		{
			// Return the BattleActorEntity of the currently selected index
			if (party != null && party.Length > 0 && index >= 0 && index < party.Length)
				return party[index].GetComponent<BattleActorEntity>();
			return null;
		}
	}

	private int index = 0;						// Current selected index in party
	private bool selected = false;				// True if a character is selected
	private int lastConfirmedIndex = -1;		// Last confirmed index when a character is selected

	void Start()
	{
		if (party == null || party.Length == 0)
		{
			Debug.LogError("Party array is empty! Assign BattleActor objects.");
			return;
		}

		index = 0;
		MoveCursorTo(index);
	}

	void Update()
	{
		if (inputLocked)
		{
			inputLocked = false;
			return; // ข้าม 1 เฟรม
		}

		// Handle input based on current state
		switch (currentState)
		{
			case BattleUIState.PartySelection:
				HandlePartySelectionInput();
				break;

			case BattleUIState.CommandSelection:
			case BattleUIState.SkillSelection:
				break;

			case BattleUIState.Targeting:
				// ไม่อ่าน input PartySelector
				break;
		}
	}

	void HandlePartySelectionInput()
	{
		// If not in the partyselection state, ignore input
		if (currentState != BattleUIState.PartySelection)
			return;

		// If not selected, allow A/D movement and selection
		if (!selected)
		{
			if (Input.GetKeyDown(KeyCode.A) && index > 0)
			{
				index--;
				MoveCursorTo(index);
			}
			if (Input.GetKeyDown(KeyCode.D) && index < party.Length - 1)
			{
				index++;
				MoveCursorTo(index);
			}
			if (Input.GetKeyDown(KeyCode.Return))
			{
				Debug.Log("Selected character: " + party[index].name);
				selected = true;				// Select current character
				lastConfirmedIndex = index;		// Remember last confirmed index
				OnSelect(party[index]);			// Trigger selection event

				// Open Command Menu
				var cmd = commandPanel.GetComponent<CommandMenuController>();
				commandPanel.SetActive(true);
				currentState = BattleUIState.CommandSelection;	// Switch to Command Selection state
				cmd.OpenMenu(this, CurrentActor);				// Initialize Command Menu with current actor
			}
		}
		else
		{
			selected = false;					// Deselect character
			if (lastConfirmedIndex >= 0)
			{
				// Return to last confirmed index
				index = lastConfirmedIndex;
				MoveCursorTo(index);
			}
			OnCancelSelection();	// Trigger cancel event
		}
	}

	// Move cursor to the specified index and highlight the corresponding UI panel
	void MoveCursorTo(int i)
	{
		// Validate index and party array
		if (party == null || party.Length == 0 || i < 0 || i >= party.Length) return;

		// Validate cursor
		if (cursor == null)
		{
			Debug.LogWarning("Cursor not assigned.");
			return;
		}

		// Move cursor to the selected party member's position
		cursor.target = party[i].selectAnchor != null ? party[i].selectAnchor : party[i].transform;

		// Highlight the selected party member's UI panel
		if (partyUI != null && partyUI.Length == party.Length)
		{
			for (int j = 0; j < partyUI.Length; j++)
			{
				partyUI[j].SetHighlight(j == i);	// Highlight only the selected index
			}
		}
	}

	void OnSelect(BattleActor actor)
	{
		// Highlight the selected party member's UI panel
		for (int j = 0; j < partyUI.Length; j++)
			partyUI[j].SetHighlight(j == index);	// Highlight only the selected index

		commandPanel.SetActive(true);	// Show Command Menu UI
	}

	void OnCancelSelection()
	{
		// Highlight the currently selected party member's UI panel
		for (int j = 0; j < partyUI.Length; j++)
			partyUI[j].SetHighlight(j == index);	// Highlight only the selected index

		commandPanel.SetActive(false);		// Hide Command Menu UI
	}

	// Called from Command or others to return to Party Selection
	public void BackToPartySelection()
	{
		currentState = BattleUIState.PartySelection;		// Switch to Party Selection state
		selected = false;                                   // Deselect character
		commandPanel.gameObject.SetActive(false);			// Close Command Menu UI
		skillMenuPanel.gameObject.SetActive(false);         // Close Skill Menu UI
		cursor.gameObject.SetActive(true);					// Show cursor
		MoveCursorTo(index);								// Move cursor to current index
		Debug.Log("Returned to Party Selection");
	}

	// Called from Command to go to Skill Selection
	public void GoToSkillSelection()
	{
		currentState = BattleUIState.SkillSelection;		// Switch to Skill Selection state
	}

	// Called from Skill to return to Command Selection
	public void BackToCommandSelection()
	{
		currentState = BattleUIState.CommandSelection;		// Switch to Command Selection state
		commandPanel.SetActive(true);						// Show Command Menu UI
		skillMenuPanel.SetActive(false);					// Close Skill Menu UI
	}
}