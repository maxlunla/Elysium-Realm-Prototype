using UnityEngine;

[RequireComponent(typeof(CharacterController))]

// Script for control the player's character movement.
// WASD for movement, Shift or Right Mouse Button to sprint.
// Sprint will be toggle with Shift or Right Mouse Button and will stop only when player stops moving (releases WASD).
public class Player_Movement_Script : MonoBehaviour
{
	[Header("Movement Settings")]
	public float walkSpeed = 4f;			// Walking speed
	public float sprintSpeed = 7f;			// Sprinting speed
	public bool canMove = true;				// Allow movement

	[Header("References")]
	public Transform cameraTransform;		// Reference to the camera for movement direction

	private CharacterController controller;	// CharacterController component (required)
	private Animator animator;				// Animator for character animations
	private bool isSprinting = false;		// Track sprint state

	private Vector3 velocity;				// Gravity
	private float gravity = -9.81f;			// Gravity value

	void Start()
	{
		// Get CharacterController component
		controller = GetComponent<CharacterController>();

		// Get Animator if exists
		animator = GetComponentInChildren<Animator>();
	}

	void Update()
	{
		// Apply only the gravity when not moving e.g., when talking with NPC or in a menu.
		if (!canMove)
		{
			// Stop horizontal movement immediately, keep gravity so the player still grounded.
			if (animator != null) animator.SetFloat("Speed", 0f);
			ApplyGravityOnly();
			return;
		}

		HandleMovement();	// Handle player movement input and movement conditions.
	}

	void HandleMovement()
	{
		// Get input from WASD.
		float h = Input.GetAxisRaw("Horizontal");			// A/D keys.
		float v = Input.GetAxisRaw("Vertical");				// W/S keys.
		Vector3 inputDir = new Vector3(h, 0, v).normalized;	// Normalized input direction in the XZ plane.

		// Start sprinting if Shift or Right Mouse Button pressed.
		if ((Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1)) && inputDir.magnitude > 0)
		{
			isSprinting = true;
		}

		// Stop sprinting only when player stops moving (releases WASD)
		if (inputDir.magnitude == 0)
		{
			isSprinting = false;
		}

		// If there's input, move the character.
		if (inputDir.magnitude > 0)
		{
			// Determine speed, sprint or walk.
			float speed = isSprinting ? sprintSpeed : walkSpeed;

			// Rotate character to face movement direction based on camera
			float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
			transform.rotation = Quaternion.Euler(0, targetAngle, 0);

			// Move character
			Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
			controller.Move(moveDir * speed * Time.deltaTime);

			// Update Animator
			if (animator != null)
			{
				if (inputDir.magnitude == 0)
				{
					// Idle
					animator.SetFloat("Speed", 0f);
				}
				else if (isSprinting)
				{
					// Run
					animator.SetFloat("Speed", 2f);
				}
				else
				{
					// Walk
					animator.SetFloat("Speed", 1f);
				}
			}
		}
		else
		{
			// Stop animation when no input
			if (animator != null)
				animator.SetFloat("Speed", 0f);
		}

		// Check gravity
		if (controller.isGrounded)
		{
			velocity.y = 0f;	// Reset vertical speed when on ground
		}
		else
		{
			velocity.y += gravity * Time.deltaTime;		// Apply gravity
		}

		controller.Move(velocity * Time.deltaTime);
	}

	void ApplyGravityOnly()
	{
		// Apply gravity only when movement is disabled.
		if (controller.isGrounded) velocity.y = 0f;
		else velocity.y += -9.81f * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}
}
