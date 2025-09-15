using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings")]
	public float moveSpeed = 6f;
	public float jumpHeight = 2.5f;
	public float gravity = -20f;

	private Vector3 velocity;
	private CharacterController controller;

	[Header("Ground Check")]
	public Transform groundCheck;      // ใส่ Empty Object ใต้เท้า Player
	public float groundDistance = 0.1f;
	public LayerMask groundMask;       // กำหนด Layer ของพื้น

	private bool isGrounded;

	[Header("Respawn Settings")]
	public Transform respawnPoint;  // ตำแหน่ง spawn ใหม่

	[Header("Damage Over Time Settings")]
	public float dotInterval = 0.2f; // ลด HP ทุก 0.2 วิ
	public int dotAmount = 5;        // ลด HP ทีละ 2
	private Coroutine dotCoroutine;
	private bool inFlashlightZone = false; // ตัวแปรเช็คว่า player อยู่ใน trigger ไหม

	void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		// ตรวจพื้นเอง
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

		if (isGrounded && velocity.y < 0)
		{
			velocity.y = -2f; // แนบพื้น
		}

		// การเคลื่อนที่แนวนอน
		float x = Input.GetAxisRaw("Horizontal");
		Vector3 move = transform.right * x;
		controller.Move(move * moveSpeed * Time.deltaTime);

		// กระโดด
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
		{
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
		}

		// แรงโน้มถ่วง
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}

	// ใช้ Trigger ตรวจพื้นที่
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DeadlyShadow"))
		{
			DieAndRespawn();
		}
		
		if (other.CompareTag("Flashlight"))
		{
			inFlashlightZone = true;
			StartDamageOverTime();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Flashlight"))
		{
			inFlashlightZone = false;   // ออกจาก trigger
			StopDamageOverTime(); // หยุดโดนลด HP
		}
	}
	
	private void DieAndRespawn()
	{
		// รีเซ็ตตำแหน่ง player ไป respawn point
		GetComponent<PlayerHealth>().TakeDamage(999);

		controller.enabled = false;          // ปิดก่อน
		transform.position = respawnPoint.position;
		velocity = Vector3.zero;            // รีเซ็ตแรง
		controller.enabled = true;           // เปิดอีกครั้ง
	}
	public void StartDamageOverTime()
	{
		if (dotCoroutine != null)
			StopCoroutine(dotCoroutine);

		dotCoroutine = StartCoroutine(DamageOverTime());
	}

	public void StopDamageOverTime()
	{
		if (dotCoroutine != null)
		{
			StopCoroutine(dotCoroutine);
			dotCoroutine = null;
		}
	}

	private IEnumerator DamageOverTime()
	{
		while (inFlashlightZone) // ตรวจสอบว่าผู้เล่นยังอยู่ใน trigger
		{
			GetComponent<PlayerHealth>().TakeDamage(dotAmount);
			yield return new WaitForSeconds(dotInterval);
		}

		dotCoroutine = null; // รีเซ็ต coroutine หลังออก trigger
	}
}