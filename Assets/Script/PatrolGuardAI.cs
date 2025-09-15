using UnityEngine;

public class PatrolGuardAI : MonoBehaviour
{
	[Header("Movement")]
	public float moveSpeed = 3f;

	[Header("Patrol Points")]
	public Transform leftPoint;
	public Transform rightPoint;
	private bool movingRight = true;

	[Header("Chase Settings")]
	public Transform player;
	public float stoppingDistance = 1.5f;

	[Header("Timers")]
	public float lostPlayerTime = 5f;       // นับถอยหลังเมื่อ player หลุด sight
	public float idleBeforeReturn = 2f;     // idle ก่อนกลับ patrol

	[Header("Flashlight")]
	public GameObject Flashlight;

	private enum AIState { Patrol, Chase, LostPlayer, IdleBeforeReturn }
	private AIState currentState = AIState.Patrol;

	private bool facingRight = true;
	private float lostTimer = 0f;
	private float idleTimer = 0f;
	private Vector3 lastSeenPosition;
	private Vector3 lastSeenDirection;

	[Header("Sight")]
	public SphereCollider sightCollider; // Sphere Collider ของ Sight

	[Header("Player State")]
	public bool isPlayerHidden = false; // ให้ HidingSpot ควบคุม

	private bool patrolMovingRight = true; // จำทิศทาง patrol ก่อน chase

	void Update()
	{
		switch (currentState)
		{
			case AIState.Patrol: Patrol(); Flashlight.SetActive(false); break;
			case AIState.Chase: Chase(); break;
			case AIState.LostPlayer: LostPlayer(); break;
			case AIState.IdleBeforeReturn: IdleBeforeReturn(); break;
		}
	}

	void Patrol()
	{
		// เดิน patrol ภายใน left/right points
		if (transform.position.x >= rightPoint.position.x)
		{
			movingRight = false;
			patrolMovingRight = movingRight; // บันทึกทิศทาง patrol
			if (facingRight) Flip();
		}
		else if (transform.position.x <= leftPoint.position.x)
		{
			movingRight = true;
			patrolMovingRight = movingRight; // บันทึกทิศทาง patrol
			if (!facingRight) Flip();
		}

		Vector3 move = (movingRight ? Vector3.right : Vector3.left) * moveSpeed * Time.deltaTime;
		transform.position += move;
	}

	void Chase()
	{
		if (isPlayerHidden)
		{
			// player หายตัว → เปลี่ยน state เป็น LostPlayer
			currentState = AIState.LostPlayer;
			lostTimer = 0f;
			return;
		}

		// เปิด flashlight
		if (!Flashlight.activeSelf) Flashlight.SetActive(true);

		// หันหน้า player
		if (player.position.x > transform.position.x && !facingRight) Flip();
		else if (player.position.x < transform.position.x && facingRight) Flip();

		// บันทึกตำแหน่งล่าสุด
		lastSeenPosition = player.position;
		lastSeenDirection = new Vector3(Mathf.Sign(player.position.x - transform.position.x), 0, 0);

		// เดินเข้าหา player แต่ไม่เกินเขต patrol และไม่ถึงตัว player
		float targetX = Mathf.Clamp(player.position.x, leftPoint.position.x, rightPoint.position.x);
		float distanceX = Mathf.Abs(transform.position.x - targetX);

		if (distanceX > stoppingDistance)
		{
			Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
		}
	}

	void LostPlayer()
	{
		lostTimer += Time.deltaTime;

		// อัปเดตตำแหน่ง player เฉพาะถ้าไม่ซ่อน
		if (!isPlayerHidden)
		{
			lastSeenPosition = player.position;
			lastSeenDirection = new Vector3(Mathf.Sign(player.position.x - transform.position.x), 0, 0);
		}

		// เดินเข้าหา lastSeenPosition
		float targetX = Mathf.Clamp(lastSeenPosition.x, leftPoint.position.x, rightPoint.position.x);
		float distanceX = Mathf.Abs(transform.position.x - targetX);

		if (distanceX > stoppingDistance)
		{
			Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
		}

		// หันหน้า lastSeenDirection
		if (lastSeenDirection.x > 0 && !facingRight) Flip();
		else if (lastSeenDirection.x < 0 && facingRight) Flip();

		Flashlight.SetActive(true);

		// ถ้า player กลับเข้ามาใน sight → Chase ต่อ และรีเซ็ต lostTimer
		if (!isPlayerHidden && sightCollider.bounds.Contains(player.position))
		{
			currentState = AIState.Chase;
			lostTimer = 0f;
		}
		else if (lostTimer >= lostPlayerTime)
		{
			lostTimer = 0f;
			idleTimer = 0f;
			currentState = AIState.IdleBeforeReturn;
		}
	}

	void IdleBeforeReturn()
	{
		idleTimer += Time.deltaTime;

		// หันตาม lastSeenDirection
		if (lastSeenDirection.x > 0 && !facingRight) Flip();
		else if (lastSeenDirection.x < 0 && facingRight) Flip();

		Flashlight.SetActive(true);

		if (idleTimer >= idleBeforeReturn)
		{
			currentState = AIState.Patrol;
			Flashlight.SetActive(false);

			// กลับไปเดิน patrol จากทิศทางก่อนถูกขัด
			movingRight = patrolMovingRight;

			// อัปเดตทิศทางหน้าของ AI ให้ตรงกับ movingRight
			if (movingRight && !facingRight) Flip();
			else if (!movingRight && facingRight) Flip();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player")) currentState = AIState.Chase;
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player") && currentState == AIState.Chase)
		{
			currentState = AIState.LostPlayer;
			lostTimer = 0f;
		}
	}

	private void Flip()
	{
		facingRight = !facingRight;
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}

	public void StartChase()
	{
		currentState = AIState.Chase;
	}

	public void StopChase()
	{
		if (currentState == AIState.Chase)
			currentState = AIState.LostPlayer;
	}
}