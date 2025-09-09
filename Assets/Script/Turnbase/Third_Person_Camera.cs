using UnityEngine;

// Script for a third-person camera with mouse control, zoom, collision detection, and player fade effect.
public class Third_Person_Camera : MonoBehaviour
{
	[Header("References")]
	public Transform target;						// Player transform
	public LayerMask collisionMask;					// Layers to detect for camera collision
	public bool inputEnabled = true; 				// Toggle input handling
	public NPC_Interaction npcInteraction;			// Reference to NPC interaction script
	public bool isInDialog = false;					// Is the player currently in a dialogue with NPC or onject.

	[Header("Camera Settings")]
	public Vector3 offset = new Vector3(0, 2f, -5f); // Default camera offset
	public float mouseSensitivity = 3f;              // Mouse rotation sensitivity
	public float minPitch = -20f;                    // Minimum vertical rotation
	public float maxPitch = 70f;                     // Maximum vertical rotation
	public float zoomSpeed = 5f;                     // Scroll wheel zoom speed
	public float minZoom = 2f;                       // Minimum zoom distance
	public float maxZoom = 8f;                       // Maximum zoom distance

	private float yaw;								// Horizontal rotation
	private float pitch;							// Vertical rotation
	private float currentZoom;						// Current zoom distance
	private Renderer[] playerRenderers;				// Player renderers for fade effect
	private bool cameraActive = true;				// Toggle camera control (ESC / Alt)

	void Start()
	{
		// Lock cursor at start of the game
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		// Initialize zoom
		currentZoom = offset.magnitude;

		// Get all renderers of the player for fade effect
		playerRenderers = target.GetComponentsInChildren<Renderer>();
	}

	void LateUpdate()
	{
		if (!inputEnabled) return; // Skip if input is disabled

		if (isInDialog)
		{
			// If in dialogue, handle camera differently
			HandleDialogueCameraInput();
			HandleCamera();
		} else
		{
			// Regular camera control
			HandleInput();
			HandleCamera();
		}
	}

	public void SetInputEnabled(bool enabled)
	{
		// Enable or disable camera input and manage cursor state.
		inputEnabled = enabled;

		if (enabled)
		{
			// If enabling input, lock cursor
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else
		{
			// If disabling input, unlock cursor
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	void HandleInput()
	{
		// Toggle mouse control with ESC or Alt keys
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.LeftAlt))
		{
			// Toggle camera active state
			cameraActive = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		if (Input.GetMouseButtonDown(0))
		{
			// Reactivate camera control on left mouse click
			cameraActive = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		if (!cameraActive) return;	// Skip if camera control is inactive

		// Mouse movement for camera rotation
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		yaw += mouseX;
		pitch -= mouseY;
		pitch = Mathf.Clamp(pitch, minPitch, maxPitch);				// Clamp vertical rotation.

		// Scroll wheel zoom
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		currentZoom -= scroll * zoomSpeed;
		currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);	// Clamp zoom distance.
	}

	void HandleCamera()
	{
		// Calculate desired rotation and position
		Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
		Vector3 desiredPos = target.position + rotation * Vector3.forward * -currentZoom + Vector3.up * offset.y;

		// Camera collision detection
		Vector3 dir = desiredPos - target.position;
		float distance = dir.magnitude;

		if (Physics.Raycast(target.position, dir.normalized, out RaycastHit hit, distance, collisionMask))
		{
			// Fade player model when camera is too close to avoid clipping
			// Move camera closer to avoid clipping
			desiredPos = target.position + dir.normalized * (hit.distance * 0.9f);

			// Fade player if camera is very close
			float alpha = Mathf.InverseLerp(minZoom, minZoom + 1.5f, hit.distance);
			SetPlayerAlpha(alpha);
		}
		else
		{
			SetPlayerAlpha(1f);
		}

		// Apply position and rotation
		transform.position = desiredPos;
		transform.rotation = rotation;
	}

	void HandleDialogueCameraInput()
	{
		// Unlock cursor and disable camera rotation when in dialogue
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		if (Input.GetMouseButton(0))
		{
			// Allow camera rotation on left mouse button hold
			float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
			float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

			yaw += mouseX;
			pitch -= mouseY;
			pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
		}

		// Allow zooming with scroll wheel
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		currentZoom -= scroll * zoomSpeed;
		currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
	}

	void SetPlayerAlpha(float alpha)
	{
		foreach (Renderer rend in playerRenderers)
		{
			foreach (Material mat in rend.materials)
			{
				if (mat.HasProperty("_Color"))
				{
					Color c = mat.color;
					c.a = alpha;
					mat.color = c;

					// Set blend mode for transparency if needed
					if (alpha < 1f)
						mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					else
						mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				}
			}
		}
	}
}