// This is a simple enumeration to represent different states of the battle UI.
public enum BattleUIState
{
	PartySelection,			// Select character in party (A/D + Enter/ESC)
	CommandSelection,		// Select action command (W/S + Enter/ESC)
	SkillSelection,			// Select skill (W/S + Enter/ESC)
	Targeting,				// Select target (A/D + Enter/ESC)
	Confirming,				// Confirm action (Enter/ESC)
	Executing				// Executing action (no input)
}