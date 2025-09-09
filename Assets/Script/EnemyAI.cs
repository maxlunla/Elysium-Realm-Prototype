using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
	public Transform player;
	public float detectionRange = 10f;
	public Light flashlight;

	private NavMeshAgent agent;
	private Vector3 startPos;

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		startPos = transform.position;
		flashlight.enabled = false;
	}

	void Update()
	{
		float distance = Vector3.Distance(transform.position, player.position);

		if (distance <= detectionRange)
		{
			// ตามผู้เล่น
			agent.SetDestination(player.position);
			flashlight.enabled = true;
		}
		else
		{
			// กลับจุดเริ่มต้น
			agent.SetDestination(startPos);
			flashlight.enabled = false;
		}
	}
}