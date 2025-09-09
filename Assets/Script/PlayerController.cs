using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float moveSpeed = 5f;
	public float jumpForce = 7f;
	public float gravity = -9.81f;

	private Vector3 velocity;  // ใช้จำแรงโน้มถ่วง
	private bool isGrounded;

	public Transform groundCheck;
	public float groundDistance = 0.2f;
	public LayerMask groundMask;

	void Update()
	{
		// ตรวจว่าผู้เล่นอยู่บนพื้น
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
		if (isGrounded && velocity.y < 0)
			velocity.y = -2f; // ค้างตัวเล็กน้อยเพื่อให้ติดพื้น

		// Movement
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		Vector3 move = new Vector3(h, 0, v);
		transform.position += move * moveSpeed * Time.deltaTime;

		// Jump
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
			velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

		// Gravity
		velocity.y += gravity * Time.deltaTime;
		transform.position += velocity * Time.deltaTime;
	}
}