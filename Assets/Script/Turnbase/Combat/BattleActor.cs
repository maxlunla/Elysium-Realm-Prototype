using UnityEngine;

// This script is attached to each character in the battle scene.
public class BattleActor : MonoBehaviour
{
	[Tooltip("Position of cursor")]
	public Transform selectAnchor;

	void Awake()
	{
		if (selectAnchor == null)
		{
			var go = new GameObject("SelectAnchor");
			go.transform.SetParent(transform);
			go.transform.localPosition = new Vector3(0, 2.3f, 0);
			selectAnchor = go.transform;
		}
	}
}
