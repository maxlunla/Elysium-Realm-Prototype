using UnityEngine;
using System.Collections;

// This script handles player movement, jumping, gravity, respawning, and damage/heal over time.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings")]
	public float moveSpeed = 10f;
	public float jumpHeight = 2f;
	public float gravity = -30f;

	private Vector3 velocity;
	private CharacterController controller;

	[Header("Ground Check")]
	public Transform groundCheck;
	public float groundDistance = 0.1f;
	public LayerMask groundMask;
	private bool isGrounded;

	[Header("Respawn Settings")]
	public Transform respawnPoint;

	[Header("Damage Over Time")]
	public float interval = 0.2f;		// Time interval for damage/heal ticks
	public int dotAmount = 5;

	private Coroutine damageCoroutine;

	private bool inFlashlightZone = false;

	public bool isInShadow = false;
	public bool IsDead = false;

	public GameObject interactText;

	void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		// Ground check
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
		if (isGrounded && velocity.y < 0)
			velocity.y = -2f;

		// Move
		float x = Input.GetAxisRaw("Horizontal");
		Vector3 move = transform.right * x;
		controller.Move(move * moveSpeed * Time.deltaTime);

		// Jump
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

		// Apply gravity
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);

		// Stop movement if dead
		if (IsDead)
		{
			controller.enabled = false;
			if (interactText != null) interactText.SetActive(false);
			return;
		}
	}

	// Trigger events
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DeadlyShadow"))
		{
			if (!isInShadow && gameObject.layer == LayerMask.NameToLayer("Default"))
				DieAndRespawn();
		}

		if (other.CompareTag("Flashlight") && gameObject.layer == LayerMask.NameToLayer("Default"))
		{
			inFlashlightZone = true;
			StartDamageOverTime();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Flashlight"))
		{
			inFlashlightZone = false;
			StopDamageOverTime();
		}
	}

	// Handle player death and respawn
	private void DieAndRespawn()
	{
		if (!isInShadow)
		{
			GetComponent<PlayerHealth>().TakeDamage(999);
			controller.enabled = false;
			transform.position = respawnPoint.position;
			velocity = Vector3.zero;
			controller.enabled = true;
		}
	}

	// Damage over time handling
	public void StartDamageOverTime()
	{
		if (damageCoroutine != null) StopCoroutine(damageCoroutine);
		damageCoroutine = StartCoroutine(DamageOverTime());
	}

	// Stop damage over time
	public void StopDamageOverTime()
	{
		if (damageCoroutine != null)
		{
			StopCoroutine(damageCoroutine);
			damageCoroutine = null;
		}
	}

	// Coroutine for applying damage over time
	private IEnumerator DamageOverTime()
	{
		while (inFlashlightZone && gameObject.layer == LayerMask.NameToLayer("Default"))
		{
			GetComponent<PlayerHealth>().TakeDamage(dotAmount);
			yield return new WaitForSeconds(interval);
		}
		damageCoroutine = null;
	}

	// Kill player externally
	public void KillPlayer()
	{
		DieAndRespawn();
	}
}
