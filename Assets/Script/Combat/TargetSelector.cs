using System.Collections.Generic;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
	public int currentIndex = 0; // index ของ target ที่เลือกอยู่
	private List<BattleActorEntity> currentTargets; // list ของ target group (enemy หรือ ally)
	private SkillData currentSkill;
	private ActorCommand currentAction;
	private BattleActorEntity currentActor;
	public CommandMenuController commandMenuController;
	public SkillMenuController skillMenuController;
	public GameObject playerCursor;

	public void StartTargeting(BattleActorEntity actor, SkillData skill = null, ActorCommand action = null)
	{
		currentActor = actor;
		currentSkill = skill;
		currentAction = action;

		currentTargets = new List<BattleActorEntity>();
		currentIndex = 0;

		playerCursor.gameObject.SetActive(false);

		// กำหนด target list ตาม Skill หรือ Action
		if (skill != null)
		{
			switch (skill.targetType)
			{
				case SkillData.TargetType.Self:
					currentTargets.Add(actor);
					break;
				case SkillData.TargetType.Single_Enemy:
				case SkillData.TargetType.Multiple_Enemies:
					currentTargets = BattleManager.Instance.enemies;
					break;
				case SkillData.TargetType.Single_Ally:
				case SkillData.TargetType.Multiple_Allies:
					currentTargets = BattleManager.Instance.players;
					break;
			}
		}
		else if (action != null)
		{
			switch (action.id)
			{
				case "Attack":
					currentTargets = BattleManager.Instance.enemies;
					break;
				case "Guard":
				case "Escape":
					currentTargets.Add(actor);
					break;
			}
		}

		RefreshCursor();
		BattleManager.Instance.partySelector.currentState = BattleUIState.Targeting;
	}

	void Update()
	{
		if (BattleManager.Instance.partySelector.currentState != BattleUIState.Targeting) return;
		if (currentTargets == null || currentTargets.Count == 0) return;

		bool canMove = false;

		// ตรวจสอบว่าต้องเลื่อน cursor หรือไม่
		if (currentSkill != null)
			canMove = currentSkill.targetType == SkillData.TargetType.Single_Enemy ||
					  currentSkill.targetType == SkillData.TargetType.Single_Ally;
		else if (currentAction != null)
			canMove = currentAction.id == "Attack";

		if (canMove)
		{
			if (Input.GetKeyDown(KeyCode.A) && currentIndex > 0)
			{
				currentIndex--;
				RefreshCursor();
			}
			if (Input.GetKeyDown(KeyCode.D) && currentIndex < currentTargets.Count - 1)
			{
				currentIndex++;
				RefreshCursor();
			}
		}

		if (Input.GetKeyDown(KeyCode.Return))
		{
			ConfirmTarget();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			CancelTargeting();
		}
	}

	void RefreshCursor()
	{
		if (currentTargets == null || currentTargets.Count == 0) return;

		bool showAll = false;

		if (currentSkill != null)
			showAll = currentSkill.targetType == SkillData.TargetType.Multiple_Enemies ||
					  currentSkill.targetType == SkillData.TargetType.Multiple_Allies;

		// ซ่อน cursor ทุกตัว
		foreach (var t in currentTargets)
			t.ShowCursor(false);

		if (showAll)
		{
			foreach (var t in currentTargets)
				t.ShowCursor(true);
		}
		else
		{
			if (currentIndex >= 0 && currentIndex < currentTargets.Count)
				currentTargets[currentIndex].ShowCursor(true);
		}
	}

	void ConfirmTarget()
	{
		var target = currentTargets[currentIndex];
		string actionName = currentSkill != null
			? currentSkill.displayName
			: currentAction?.displayName ?? "Attack";

		int tpCost = currentSkill != null
			? currentSkill.tpCost
			: currentAction?.tpCost ?? 0;

		int mpCost = currentSkill != null
			? currentSkill.mpCost
			: currentAction?.mpCost ?? 0;

		Debug.Log($"{currentActor.characterName} used {actionName} on {target.characterName} " +
				  $"(TP: {tpCost}, MP: {mpCost})");

		// TODO: execute effect

		foreach (var t in currentTargets)
			t.ShowCursor(false);

		// เพิ่มเข้า Action Queue
		BattleManager.Instance.AddToActionQueue(currentActor, currentSkill, currentAction, target);

		// เช็คว่า AP หรือ Action หมดหรือยัง
		if (BattleManager.Instance.currentAP <= 0 || BattleManager.Instance.currentAction <= 0)
		{
			playerCursor.gameObject.SetActive(false);

			BattleManager.Instance.actionQueuePanel.executeControlsPanel.SetActive(true);
			commandMenuController.CloseMenu();
			skillMenuController.CloseMenu();
			BattleManager.Instance.partySelector.currentState = BattleUIState.Confirming;
		}
		else
		{
			// ยังเหลือ AP/Action → กลับไป Party Selection
			BattleManager.Instance.actionQueuePanel.executeControlsPanel.SetActive(false);
			commandMenuController.SetLocked(true);
			BattleManager.Instance.partySelector.inputLocked = true;
			BattleManager.Instance.partySelector.currentState = BattleUIState.PartySelection;
			BattleManager.Instance.partySelector.BackToPartySelection();
		}

		BattleManager.Instance.skillMenuController.HideSkillDescriptionPanel();
		playerCursor.gameObject.SetActive(true);
	}

	void CancelTargeting()
	{
		foreach (var t in currentTargets)
			t.ShowCursor(false);

		//BattleManager.Instance.skillMenuController.HideSkillDescriptionPanel();
		BattleManager.Instance.skillMenuController.lockedInput(true);
		BattleManager.Instance.partySelector.inputLocked = true;

		if (currentSkill != null)
		{
			// Targeting มาจาก Skill → กลับไปหน้า SkillSelection
			BattleManager.Instance.partySelector.currentState = BattleUIState.SkillSelection;
			BattleManager.Instance.partySelector.GoToSkillSelection();
		}
		else
		{
			// Targeting มาจาก Action → กลับไปหน้า CommandSelection
			BattleManager.Instance.partySelector.currentState = BattleUIState.CommandSelection;
			BattleManager.Instance.partySelector.BackToCommandSelection();
		}

		commandMenuController.SetLocked(true);
		playerCursor.gameObject.SetActive(true);
	}
}