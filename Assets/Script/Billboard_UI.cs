using UnityEngine;

// Make the UI (World Space) look at the camera all the time.
public class Billboard_UI : MonoBehaviour
{
	private Camera mainCamera;

	void Start()
	{
		mainCamera = Camera.main;
	}

	void LateUpdate()
	{
		transform.forward = mainCamera.transform.forward;
	}
}