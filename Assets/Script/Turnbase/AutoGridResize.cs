using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AutoGridResize : MonoBehaviour
{
	public int columns = 12; // Max columns

	void Update()
	{
		RectTransform rt = GetComponent<RectTransform>();
		GridLayoutGroup grid = GetComponent<GridLayoutGroup>();

		float width = rt.rect.width;
		float spacing = grid.spacing.x * (columns - 1);

		float cellSize = (width - spacing) / columns;

		grid.cellSize = new Vector2(cellSize, cellSize); // Make cells square
	}
}