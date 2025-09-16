using UnityEngine;
using System.Collections;

// This script toggles a set of GameObjects on and off in an alternating pattern at set intervals.
public class AutoToggleObjects : MonoBehaviour
{
	[Header("Objects to Toggle")]
	public GameObject[] objects;			// Array of GameObjects to toggle

	[Header("Timing")]
	public float toggleInterval = 2f;		// Interval in seconds between toggles

	private bool toggleEven = true;

	void Start()
	{
		StartCoroutine(ToggleRoutine());	// Start the toggling coroutine
	}

	// Coroutine to toggle objects on and off
	IEnumerator ToggleRoutine()
	{
		// Loop indefinitely to toggle objects on and off at set intervals
		while (true)
		{
			// Loop through each object and set its active state based on the current toggleEven flag
			for (int i = 0; i < objects.Length; i++)
			{
				if (toggleEven)
				{
					// Activate even-indexed objects (1st, 3rd, etc.) and deactivate odd-indexed ones
					objects[i].SetActive((i + 1) % 2 == 0);
				}
				else
				{
					// Activate odd-indexed objects (2nd, 4th, etc.) and deactivate even-indexed ones
					objects[i].SetActive((i + 1) % 2 != 0);
				}
			}

			toggleEven = !toggleEven;	// Switch the toggle flag for the next iteration
			yield return new WaitForSeconds(toggleInterval);	// Wait for the specified interval before the next toggle
		}
	}
}
