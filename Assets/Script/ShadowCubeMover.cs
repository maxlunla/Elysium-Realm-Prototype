using UnityEngine;

// This script moves a cube back and forth between two points.
public class ShadowCubeMover : MonoBehaviour
{
	[Header("Move Points")]
	public Transform pointA;	// First point
	public Transform pointB;	// Second point

	[Header("Movement")]
	public float speed = 2f;	// Movement speed
	private Vector3 target;		// Current target point

	void Start()
	{
		target = pointB.position;	// First target to go (to point B)
	}

	void Update()
	{
		// Move towards the target point
		transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

		// If close enough to the target, switch to the other point
		if (Vector3.Distance(transform.position, target) < 0.05f)
		{
			if (target == pointA.position)
				target = pointB.position;
			else
				target = pointA.position;
		}
	}
}
