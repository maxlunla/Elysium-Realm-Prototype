using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script manages the action queue panel UI during battle

public class ActionQueuePanel : MonoBehaviour
{
	[Header("Action Icons")]
	public Image[] actionIcons;					// Array of UI Image components to display action icons

	[Header("AP/Action Texts")]
	public TMP_Text apText;						// Text component to display current AP
	public TMP_Text actionText;					// Text component to display current action

	[Header("Execute Controls Panel")]
	public GameObject executeControlsPanel;		// Panel for execute controls (e.g., Confirm, Cancel)
}