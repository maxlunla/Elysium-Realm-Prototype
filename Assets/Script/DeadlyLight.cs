using UnityEngine;

// This script kills the player if they enter a deadly light area.
public class DeadlyLight : MonoBehaviour
{
	[SerializeField]
	private Collider lightCollider;		// Collider of the deadly light area

	[SerializeField]
	private PlayerController player;	// Reference to the PlayerController script

	private void Start()
	{
		lightCollider = GetComponent<Collider>();
		lightCollider.isTrigger = true;
		player = FindObjectOfType<PlayerController>();
	}

	private void Update()
	{
		// Do noting if player reference is missing
		if (player == null) return;

		// If the player is within the bounds of the light collider and not in shadow, kill the player
		if (lightCollider.bounds.Contains(player.transform.position))
		{
			if (player.isInShadow == false)
			{
				player.KillPlayer();
			}
		}
	}
}
