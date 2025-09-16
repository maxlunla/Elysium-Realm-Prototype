using UnityEngine;

// This script handles the patrol and chase behavior of the guard AI.
public class PatrolGuardAI : MonoBehaviour
{
	[Header("Movement")]
	public float moveSpeed = 3f;					// Speed of movement in patrol and chase

	[Header("Patrol Points")]
	public Transform leftPoint;						// Left boundary of patrol
	public Transform rightPoint;					// Right boundary of patrol
	private bool movingRight = true;				// Direction of patrol movement

	[Header("Chase Settings")]
	public Transform player;						// Reference to the player transform
	public float stoppingDistance = 1.5f;			// Distance to stop before reaching the player

	[Header("Timers")]
	public float lostPlayerTime = 5f;				// Time to search for player after losing sight before returning to patrol
	public float idleBeforeReturn = 2f;				// Time to idle before returning to patrol (after losing player)

	[Header("Flashlight")]
	public GameObject Flashlight;					// Flashlight GameObject to enable/disable

	private enum AIState { Patrol, Chase, LostPlayer, IdleBeforeReturn }	// States of the AI
	private AIState currentState = AIState.Patrol;	// Current state of the AI

	private bool facingRight = true;				// Direction the AI is facing
	private float lostTimer = 0f;					// Timer for lost player state
	private float idleTimer = 0f;					// Timer for idle before return state
	private Vector3 lastSeenPosition;				// Last known position of the player
	private Vector3 lastSeenDirection;				// Direction to the last known position of the player

	[Header("Sight")]
	public SphereCollider sightCollider;			// Collider used for sight detection (should be a trigger)

	[Header("Player State")]
	public bool isPlayerHidden = false;				// Whether the player is currently hidden

	private bool patrolMovingRight = true;			// Direction of patrol movement to return to after chase

	void Update()
	{
		// AI state machine
		switch (currentState)
		{
			case AIState.Patrol: Patrol(); Flashlight.SetActive(false); break;
			case AIState.Chase: Chase(); break;
			case AIState.LostPlayer: LostPlayer(); break;
			case AIState.IdleBeforeReturn: IdleBeforeReturn(); break;
		}
	}

	// Patrol state movement and logic
	void Patrol()
	{
		// Patrol between left and right points and flip direction at boundaries
		// If reached boundary, change direction and flip if needed
		// If facing right and moving left, or facing left and moving right, flip
		if (transform.position.x >= rightPoint.position.x)
		{
			movingRight = false;				// Set direction to left
			patrolMovingRight = movingRight;	// Save patrol direction to return to after chase
			if (facingRight) Flip();			// Flip if facing right
		}
		else if (transform.position.x <= leftPoint.position.x)
		{
			movingRight = true;					// Set direction to right
			patrolMovingRight = movingRight;	// Save patrol direction to return to after chase 
			if (!facingRight) Flip();			// Flip if facing left
		}

		// Move in the current direction at moveSpeed
		Vector3 move = (movingRight ? Vector3.right : Vector3.left) * moveSpeed * Time.deltaTime;
		transform.position += move;
	}

	// Chase state movement and logic
	void Chase()
	{
		// If player is hidden, switch to LostPlayer state
		if (isPlayerHidden)
		{
			currentState = AIState.LostPlayer;	// Switch to LostPlayer state
			lostTimer = 0f;						// Reset lost timer
			return;
		}

		// Active flashlight during chase
		if (!Flashlight.activeSelf) Flashlight.SetActive(true);

		// Face the player
		if (player.position.x > transform.position.x && !facingRight) Flip();
		else if (player.position.x < transform.position.x && facingRight) Flip();

		// Save player last seen position and direction
		lastSeenPosition = player.position;
		lastSeenDirection = new Vector3(Mathf.Sign(player.position.x - transform.position.x), 0, 0);

		// Move towards the player but stop at stoppingDistance away from player on the x-axis
		// Clamp targetX to stay within patrol boundaries
		float targetX = Mathf.Clamp(player.position.x, leftPoint.position.x, rightPoint.position.x);
		float distanceX = Mathf.Abs(transform.position.x - targetX);

		if (distanceX > stoppingDistance)
		{
			Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
		}
	}

	// LostPlayer state movement and logic
	void LostPlayer()
	{
		lostTimer += Time.deltaTime;		// Increment lost timer

		// Update last seen position if player is visible and not hidden
		if (!isPlayerHidden)
		{
			lastSeenPosition = player.position;
			lastSeenDirection = new Vector3(Mathf.Sign(player.position.x - transform.position.x), 0, 0);
		}

		// If lostTimer exceeds lostPlayerTime, switch to IdleBeforeReturn state
		float targetX = Mathf.Clamp(lastSeenPosition.x, leftPoint.position.x, rightPoint.position.x);				// Clamp last seen position within patrol boundaries
		float distanceX = Mathf.Abs(transform.position.x - targetX);												// Distance to last seen position

		// Move towards last seen position but stop at stoppingDistance away on the x-axis
		if (distanceX > stoppingDistance)
		{
			Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);					// Target position clamped within patrol boundaries
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);	// Move towards target position
		}

		// Face the last seen direction
		if (lastSeenDirection.x > 0 && !facingRight) Flip();				// Face right
		else if (lastSeenDirection.x < 0 && facingRight) Flip();			// Face left

		Flashlight.SetActive(true);

		// If player becomes visible again, switch back to Chase state
		if (!isPlayerHidden && sightCollider.bounds.Contains(player.position))
		{
			currentState = AIState.Chase;		// Switch back to Chase state
			lostTimer = 0f;						// Reset lost timer
		}
		else if (lostTimer >= lostPlayerTime)
		{
			// If exceeded lostPlayerTime, switch to IdleBeforeReturn state
			lostTimer = 0f;
			idleTimer = 0f;
			currentState = AIState.IdleBeforeReturn;
		}
	}

	// Idle before returning to patrol state
	void IdleBeforeReturn()
	{
		idleTimer += Time.deltaTime;		// Increment idle timer

		// Face the last seen direction
		if (lastSeenDirection.x > 0 && !facingRight) Flip();		// Face right
		else if (lastSeenDirection.x < 0 && facingRight) Flip();	// Face left

		Flashlight.SetActive(true);

		// After idleBeforeReturn time, switch back to Patrol state
		if (idleTimer >= idleBeforeReturn)
		{
			currentState = AIState.Patrol;
			Flashlight.SetActive(false);

			// Return to patrol direction
			movingRight = patrolMovingRight;

			// Face patrol direction
			if (movingRight && !facingRight) Flip();
			else if (!movingRight && facingRight) Flip();
		}
	}

	// Trigger detection for player entering/exiting sight range
	private void OnTriggerEnter(Collider other)
	{
		// If player enters sight range, switch to Chase state
		if (other.CompareTag("Player")) currentState = AIState.Chase;
	}

	// If player exits sight range while chasing, switch to LostPlayer state
	private void OnTriggerExit(Collider other)
	{
		// Only switch to LostPlayer if currently chasing
		if (other.CompareTag("Player") && currentState == AIState.Chase)
		{
			currentState = AIState.LostPlayer;
			lostTimer = 0f;
		}
	}

	// Flip the enemy's facing direction
	private void Flip()
	{
		facingRight = !facingRight;				// Toggle facing direction
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;			// Flip the scale on x-axis
	}

	// Start chasing the player (can be called externally)
	public void StartChase()
	{
		currentState = AIState.Chase;
	}

	// Stop chasing the player (can be called externally)
	public void StopChase()
	{
		if (currentState == AIState.Chase)
			currentState = AIState.LostPlayer;
	}
}