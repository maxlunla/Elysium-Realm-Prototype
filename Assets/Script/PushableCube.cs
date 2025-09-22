using UnityEngine;

// This script allows a cube to be pushed between snap points when the player is in specific interaction zones and presses a key.
public class PushableCubeSnap : MonoBehaviour
{
	[Header("Snap Settings")]
	public Transform[] snapPoints;			// Array of snap points for the cube to move between
	public float pushSpeed = 5f;			// Speed at which the cube moves
	private int currentIndex = 0;			// Current index of the snap point
	private Vector3 targetPosition;			// Target position to move towards
	private bool isMoving = false;			// Is the cube currently moving?

	[Header("Interaction Zones")]
	public bool playerInLeftZone = false;	// Is the player in the left interaction zone?
	public bool playerInRightZone = false;	// Is the player in the right interaction zone?
	public KeyCode pushKey = KeyCode.F;		// Key to press for pushing the cube

	void Start()
	{
		// Start at the first snap point if available
		if (snapPoints.Length > 0)
		{
			currentIndex = 0;
			transform.position = snapPoints[currentIndex].position;	// Start at the first snap point
			targetPosition = transform.position;	// Set initial target position to current position
		}
	}

	void Update()
	{
		// If the cube is moving, move it towards the target position
		if (isMoving)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, pushSpeed * Time.deltaTime);

			// Stop moving if close enough to the target
			if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
				isMoving = false;
		}
		// Check for player input to push the cube if not already moving
		else if (Input.GetKeyDown(pushKey))
		{
			int newIndex = currentIndex;	// Default to current index

			// Determine new index based on player position in interaction zones
			if (playerInLeftZone) newIndex = currentIndex + 1;
			else if (playerInRightZone) newIndex = currentIndex - 1;

			// Avoid going out of bounds of the snapPoints array
			if (newIndex < 0) newIndex = 0;
			if (newIndex >= snapPoints.Length) newIndex = snapPoints.Length - 1;

			// If the index has changed, update target position and start moving
			if (newIndex != currentIndex)
			{
				currentIndex = newIndex;
				targetPosition = snapPoints[currentIndex].position;
				isMoving = true;
			}
		}
	}

	// Use with the trigger of LeftZone from the CubeInteractZone script
	public void SetLeftZone(bool value) 
	{ 
		playerInLeftZone = value; 
	}

	// Use with the trigger of RightZone from the CubeInteractZone script
	public void SetRightZone(bool value) 
	{ 
		playerInRightZone = value; 
	}
}
