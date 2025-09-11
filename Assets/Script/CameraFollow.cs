using UnityEngine;

// This script attach to the camera to follow the player smoothly
public class CameraFollow : MonoBehaviour
{
	public Transform target;          // ผู้เล่น
	public Vector3 offset;            // ระยะห่างกล้อง
	public float smoothSpeed = 0.125f;

	void LateUpdate()
	{
		Vector3 desiredPosition = target.position + offset;
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
		transform.position = smoothedPosition;
	}
}