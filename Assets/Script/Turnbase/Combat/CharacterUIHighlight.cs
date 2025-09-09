using UnityEngine;
using UnityEngine.UI;

// This script handles highlighting the character UI panel when selected.
public class CharacterUIHighlight : MonoBehaviour
{
	public Image panelImage;						// Party Member panel (background image).
	public Color normalColor = Color.white;			// Normal color of the panel.
	public Color highlightColor = Color.yellow;		// Highlight color of the panel.

	public void SetHighlight(bool isOn)
	{
		// Change the panel color based on selection state.
		if (panelImage != null)
			panelImage.color = isOn ? highlightColor : normalColor;
	}
}
