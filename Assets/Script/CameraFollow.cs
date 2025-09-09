using UnityEngine;

// This script attach to the camera to follow the player smoothly
public class CameraFollow : MonoBehaviour
{
	public Transform target;						// the player transform
	public Vector3 offset = new Vector3(0, 4, -4);	// the offset from the player
	public float smoothTime = 0.3f;					// the smooth time

	private Vector3 velocity = Vector3.zero;		// the velocity for SmoothDamp

	void LateUpdate()
	{
		if (target == null) return;	// If no target, do nothing

		// Set the position and rotation of the camera
		Vector3 desiredPosition = target.position + offset;
		transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

		transform.rotation = Quaternion.Euler(20, 0, 0);	// Set a fixed angle
		Camera.main.fieldOfView = 50;						// Set a fixed FOV
	}
}