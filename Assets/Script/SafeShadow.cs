using UnityEngine;

// This script marks the player as being in a safe shadow area, prevent them from being killed by deadly light.
public class SafeShadow : MonoBehaviour
{
	[SerializeField]
	private Collider shadowCollider;	// Collider of the shadow area

	[SerializeField]
	private PlayerController player;	// Reference to the PlayerController script

	private void Start()
	{
		shadowCollider = GetComponent<Collider>();
		shadowCollider.isTrigger = true;
		player = FindObjectOfType<PlayerController>();
	}

	private void OnTriggerEnter(Collider other)
	{
		// If the player enters the shadow area, mark them as being in shadow
		if (other.CompareTag("Player"))
		{
			PlayerController player = other.GetComponent<PlayerController>();

			// Set player isInShadow to true if player reference is valid
			if (player != null)
			{
				player.isInShadow = true;
			}
		}
	}

	private void Update()
	{
		// Do noting if player reference is missing
		if (player == null) return;

		// If the player is within the bounds of the shadow collider, set isInShadow to true
		if (shadowCollider.bounds.Contains(player.transform.position))
		{
			player.isInShadow = true;
		}
		else
		{
			// If not in bounds, set isInShadow to false
			if (player.isInShadow) player.isInShadow = false;
		}
	}

	// When the player exits the shadow area, mark them as not being in shadow
	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PlayerController player = other.GetComponent<PlayerController>();
			if (player != null)
				player.isInShadow = false;
		}
	}
}
