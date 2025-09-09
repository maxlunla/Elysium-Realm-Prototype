using System.Collections;
using UnityEngine;

public class ExecuteController : MonoBehaviour
{
	public GameObject executePanel; // Panel for execute controls (e.g., Confirm, Cancel)
	public IEnumerator ExecuteQueuedActions()
	{
		var queue = BattleManager.Instance.GetQueuedActions(); // Take queued actions from BattleManager

		foreach (var item in queue)
		{
			yield return StartCoroutine(ExecuteAction(item));
		}

		// After all actions are executed
		BattleManager.Instance.ClearAllQueuedActions();
		//BattleManager.Instance.partySelector.currentState = BattleUIState.EnemyTurn;
		BattleManager.Instance.partySelector.currentState = BattleUIState.PartySelection;

		BattleManager.Instance.partySelector.inputLocked = true;
		BattleManager.Instance.partySelector.BackToPartySelection();

		executePanel.SetActive(false);
	}

	private IEnumerator ExecuteAction(BattleManager.ActionQueueItem item)
	{
		// 1. Show animation / effect
		if (item.skill != null)
		{
			Debug.Log($"{item.actor.characterName} uses {item.skill.displayName} on {item.target.characterName}");
			// TODO: Play skill animation
		}
		else if (item.action != null)
		{
			Debug.Log($"{item.actor.characterName} uses {item.action.displayName} on {item.target.characterName}");
			// TODO: Play action animation
		}

		yield return new WaitForSeconds(0.5f); // Wait for animation duration

		// 2. Apply effect
		int damage = CalculateDamage(item); // Calculate damage
		item.target.TakeDamage(damage);

		// 3. Update UI
		// TODO: อัปเดต HP bar, MP bar

		yield return new WaitForSeconds(0.2f); // Wait a bit before next action
	}

	private int CalculateDamage(BattleManager.ActionQueueItem item)
	{
		// Simple damage calculation: ATK - DEF
		int atk = item.actor.ATK;
		int def = item.target.DEF;

		int dmg = Mathf.Max(1, atk - def);
		return dmg;
	}
}