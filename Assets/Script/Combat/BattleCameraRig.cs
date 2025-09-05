using System.Collections.Generic;
using UnityEngine;

// This script controls a camera rig that frames both player and enemy targets in a battle scene.
public class BattleCameraRig : MonoBehaviour
{
	[Header("Targets")]
	public Transform playersRoot;		// Root Transform of player characters
	public Transform enemiesRoot;		// Root Transform of enemy characters

	[Header("View Config")]
	[Range(10f, 80f)] public float fov = 50f;			// Field of View
	[Range(10f, 60f)] public float elevation = 20f;		// Camera elevation angle
	[Range(0f, 80f)] public float yawFromRight = 35f;	// Yaw angle from the right side
	public float padding = 1.15f;						// Padding factor
	public float minDistance = 6f;						// Minimum camera distance
	public Vector3 worldOffset = Vector3.zero;			// World offset for camera target
	public float smoothSpeed = 5f;						// Smoothing speed for camera movement

	[Header("Debug")]
	public bool liveUpdate = true;						// Whether to update camera every frame in editor

	Camera _cam;		// Camera component reference

	void Awake()
	{
		// Initialize camera component and set FOV
		_cam = GetComponent<Camera>();
		if (_cam != null) _cam.fieldOfView = fov;
	}

	void LateUpdate()
	{
		// Update camera position and rotation smoothly
		if (!liveUpdate) return;
		FrameAll();
	}

	public void FrameAll()
	{
		// Check if roots are assigned
		if (playersRoot == null || enemiesRoot == null) return;

		// 1) Find combined bounds of players and enemies, then find center point
		var bounds = GetCombinedBoundsFromTransform(playersRoot, enemiesRoot);
		if (bounds.size == Vector3.zero) return;

		Vector3 center = bounds.center + worldOffset;

		// 2) Find combined bounds of players only, then find center point
		var playerBounds = GetCombinedBoundsFromTransform(playersRoot);
		Vector3 playerCenter = playerBounds.center;

		// 3) Calculate required camera distance based on FOV and bounds
		float halfHeight = Mathf.Max(bounds.extents.y, 1f);
		float halfWidth = Mathf.Max(bounds.extents.x, 1f);

		float fovRad = Mathf.Deg2Rad * fov;
		float distByHeight = halfHeight / Mathf.Tan(fovRad * 0.5f);

		float aspect = Mathf.Max(_cam != null ? _cam.aspect : (16f / 9f), 1f);
		float horizFov = 2f * Mathf.Atan(Mathf.Tan(fovRad * 0.5f) * aspect);
		float distByWidth = halfWidth / Mathf.Tan(horizFov * 0.5f);

		float baseDistance = Mathf.Max(distByHeight, distByWidth) * padding;
		baseDistance = Mathf.Max(baseDistance, minDistance);

		// 4) Calculate direction from player center to battle center, then apply yaw offset
		Vector3 dirToCenter = (center - playerCenter).normalized;

		// Rotate direction by yawFromRight angle
		dirToCenter = Quaternion.Euler(0, -yawFromRight, 0) * dirToCenter;

		// 5) Calculate target camera position
		Vector3 targetCamPos = playerCenter - dirToCenter * baseDistance;
		targetCamPos.y += Mathf.Tan(Mathf.Deg2Rad * elevation) * baseDistance;

		// 6) Smoothly move camera to target position and look at center
		transform.position = Vector3.Lerp(transform.position, targetCamPos, Time.deltaTime * smoothSpeed);
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(center - targetCamPos, Vector3.up), Time.deltaTime * smoothSpeed);
	}

	// Get combined bounds from multiple root Transforms by encapsulating all child positions (Use Transform center instead of Renderer)
	Bounds GetCombinedBoundsFromTransform(params Transform[] roots)
	{
		bool hasBounds = false;
		Bounds b = new Bounds(Vector3.zero, Vector3.zero);

		foreach (var root in roots)
		{
			var trs = root.GetComponentsInChildren<Transform>();
			foreach (var t in trs)
			{
				if (t == root) continue;
				if (!hasBounds)
				{
					b = new Bounds(t.position, Vector3.one * 0.1f);
					hasBounds = true;
				}
				else b.Encapsulate(t.position);
			}
		}
		return b;
	}
}
