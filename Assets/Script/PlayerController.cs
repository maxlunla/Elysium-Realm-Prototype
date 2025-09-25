using UnityEngine;
using System.Collections;

// This script handles player movement, jumping, gravity, respawning, and damage over time when in a specific trigger zone.
// Requires a CharacterController component on the same GameObject.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings")]
	public float moveSpeed = 10f;			// Move speed of the player
	public float jumpHeight = 2f;			// Jump height of the player
	public float gravity = -30f;			// Gravity force applied to the player

	private Vector3 velocity;				// Velocity vector for movement
	private CharacterController controller;	// Reference to the CharacterController component

	[Header("Ground Check")]
	public Transform groundCheck;			// Check if the player is grounded (empty GameObject at the player's feet)
	public float groundDistance = 0.1f;		// Radius of the sphere to check for ground
	public LayerMask groundMask;			// LayerMask to define what is ground (e.g., "Ground" layer)

	private bool isGrounded;				// Is the player grounded?

	[Header("Respawn Settings")]
	public Transform respawnPoint;			// Respawn point (empty GameObject)

	[Header("Damage Over Time Settings")]
	public float dotInterval = 0.2f;		// Interval for damage over time
	public int dotAmount = 5;				// Amount of damage per interval
	private Coroutine dotCoroutine;			// Reference to the active DOT coroutine
	private bool inFlashlightZone = false;  // Is the player in the flashlight damage zone?
	public bool isInShadow = false;			// Controled by SafeShadow script

	public bool IsDead = false;			// Is the player dead?
	public GameObject interactText;		// Reference to the interact text UI element

	void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		// Check if grounded by casting a sphere at the groundCheck position
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

		// If player is grounded and falling, then reset downward velocity to a small negative value to keep them grounded smoothly
		if (isGrounded && velocity.y < 0)
		{
			velocity.y = -2f;
		}

		float x = Input.GetAxisRaw("Horizontal");				// Get horizontal input (A/D or Left/Right arrows)
		Vector3 move = transform.right * x;						// Calculate movement direction
		controller.Move(move * moveSpeed * Time.deltaTime);		// Move the player

		// Jumping mechanic - only allow jump if grounded and space key is pressed
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
		{
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);	// Calculate jump velocity
		}

		// Apply gravity to vertical velocity
		velocity.y += gravity * Time.deltaTime;			// Apply gravity
		controller.Move(velocity * Time.deltaTime);		// Move the player based on velocity

		// If player is dead, ensure they cannot move and hide the interact text
		if (IsDead)
		{
			controller.enabled = false;		// Disable controller to prevent movement
			interactText.SetActive(false);	// Hide interact text
			return;							// Exit update early
		}
	}

	// Handle trigger events for deadly shadows and flashlight zones
	private void OnTriggerEnter(Collider other)
	{
		// If player collides with a deadly shadow, they die and respawn
		if (other.CompareTag("DeadlyShadow"))
		{
			if (!isInShadow && gameObject.layer == LayerMask.NameToLayer("Default"))
			{
				Debug.Log("Player hit a deadly shadow and will respawn.");
				DieAndRespawn();
			}
		}
		
		// If player enters a flashlight zone, start taking damage over time
		if (other.CompareTag("Flashlight") && gameObject.layer == LayerMask.NameToLayer("Default"))
		{
			inFlashlightZone = true;
			StartDamageOverTime();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		// If player exits a flashlight zone, stop taking damage over time
		if (other.CompareTag("Flashlight"))
		{
			inFlashlightZone = false;
			StopDamageOverTime();
		}
	}
	
	private void DieAndRespawn()
	{
		if (!isInShadow)
		{
			// Inflict fatal damage to the player
			GetComponent<PlayerHealth>().TakeDamage(999);
			controller.enabled = false;                     // Disable controller to avoid issues during teleport
			transform.position = respawnPoint.position;     // Teleport player to respawn point
			velocity = Vector3.zero;                        // Reset velocity
			controller.enabled = true;                      // Re-enable controller
		}
	}

	public void StartDamageOverTime()
	{
		// If a DOT coroutine is already running, stop it before starting a new one
		if (dotCoroutine != null)
			StopCoroutine(dotCoroutine);

		dotCoroutine = StartCoroutine(DamageOverTime());	// Start the DOT coroutine
	}

	public void StopDamageOverTime()
	{
		// Stop the DOT coroutine if it's running
		if (dotCoroutine != null)
		{
			StopCoroutine(dotCoroutine);
			dotCoroutine = null;
		}
	}

	private IEnumerator DamageOverTime()
	{
		// Continuously apply damage while the player is in the flashlight zone
		while (inFlashlightZone && gameObject.layer == LayerMask.NameToLayer("Default"))
		{
			GetComponent<PlayerHealth>().TakeDamage(dotAmount);		// Inflict damage
			yield return new WaitForSeconds(dotInterval);			// Wait for the specified interval
		}

		dotCoroutine = null;	// Clear the coroutine reference when done
	}

	public void KillPlayer()
	{
		DieAndRespawn();	// Public method to kill the player and respawn
	}
}