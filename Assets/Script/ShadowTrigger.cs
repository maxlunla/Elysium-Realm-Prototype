using UnityEngine;
using System.Collections.Generic;

// This script handles the behavior of Light Cubes when they enter or exit a shadow trigger area.
public class ShadowTrigger : MonoBehaviour
{
	[Header("Light Cube Tag")]
	public string lightCubeTag = "DeadlyShadow";	// Tag of the Light Cube objects to detect

	private HashSet<GameObject> lightCubesInTrigger = new HashSet<GameObject>();	// Set of Light Cubes currently in the trigger area

	private void OnTriggerEnter(Collider other)
	{
		// Check if the entering object is a Light Cube
		if (other.CompareTag(lightCubeTag))
		{
			GameObject light = other.gameObject;	// Get the Light Cube game object

			// If the Light Cube is not already in the set, add it and change its visibility
			if (!lightCubesInTrigger.Contains(light))
			{
				lightCubesInTrigger.Add(light);

				// Disable the MeshRenderer of the parent Light Cube (trigger still active)
				var mr = light.GetComponent<MeshRenderer>();
				if (mr != null) mr.enabled = false;

				// Active all child objects of the Light Cube
				for (int i = 0; i < light.transform.childCount; i++)
				{
					light.transform.GetChild(i).gameObject.SetActive(true);
				}
			}
		}
	}

	// When a Light Cube exits the trigger area, restore its original visibility
	private void OnTriggerExit(Collider other)
	{
		// Check if the exiting object is a Light Cube
		if (other.CompareTag(lightCubeTag))
		{
			GameObject light = other.gameObject;	// Get the Light Cube game object

			// If the Light Cube is in the set, remove it and restore its visibility
			if (lightCubesInTrigger.Contains(light))
			{
				lightCubesInTrigger.Remove(light);

				// Enable the MeshRenderer of the parent Light Cube
				var mr = light.GetComponent<MeshRenderer>();
				if (mr != null) mr.enabled = true;

				// Deactivate all child objects of the Light Cube
				for (int i = 0; i < light.transform.childCount; i++)
				{
					light.transform.GetChild(i).gameObject.SetActive(false);
				}
			}
		}
	}
}