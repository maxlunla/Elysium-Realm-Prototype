using UnityEngine;

// This script makes a cursor follow a target transform with an offset and smooth movement.
public class SelectionCursor : MonoBehaviour
{
	public Transform target;							// The target to follow
	public Vector3 offset = new Vector3(0, 0.25f, 0);	// Offset from the target position
	public float followSpeed = 20f;						// Speed of following

	void LateUpdate()
	{
		// If no target, do nothing
		if (target == null) return;

		// Move towards the target position with offset
		Vector3 wanted = target.position + offset;
		transform.position = Vector3.Lerp(transform.position, wanted, Time.deltaTime * followSpeed);

		// Face to the camera, if needed (Sprite) or keep a fixed rotation (3D)
		// transform.forward = Camera.main.transform.forward;
	}
}
