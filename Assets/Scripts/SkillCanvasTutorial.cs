using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCanvasTutorial : SkillCanvas
{

	public void HandleSkillsT(bool isEnemyAdjacent, bool isEnemyVulnerable, bool isEnemyDefensive, bool isEnemyShootingArrow, bool isEnemyIdle, SkillType enemyActionType, bool sidestepUsed, bool isEnemyAnswered)
	{
		List<SkillButton> availableSkills = new List<SkillButton>();
		SkillButton availableKillingBlow = null;

		if (!isEnemyAnswered && isEnemyAdjacent && !isEnemyVulnerable)
		{
			SwiftAttackSkillButton.gameObject.SetActive(true);
			availableSkills.Add(SwiftAttackSkillButton);
			HandleButtonIconsForSkill(Skill.SwiftAttack, enemyActionType, SwiftAttackSkillButton);
		}
		else
		{
			SwiftAttackSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && isEnemyAdjacent && !isEnemyDefensive && !isEnemyShootingArrow && !isEnemyIdle)
		{
			BlockSkillButton.gameObject.SetActive(true);
			availableSkills.Add(BlockSkillButton);
			HandleButtonIconsForSkill(Skill.Block, enemyActionType, BlockSkillButton);
		}
		else
		{
			BlockSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && isEnemyShootingArrow && sidestepUsed)
		{
			DeflectArrowSkillButton.gameObject.SetActive(true);
			availableSkills.Add(DeflectArrowSkillButton);
			HandleButtonIconsForSkill(Skill.DeflectArrow, enemyActionType, DeflectArrowSkillButton);
		}
		else
		{
			DeflectArrowSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && isEnemyVulnerable && isEnemyAdjacent)
		{
			KillingBlowSkillButton.gameObject.SetActive(true);
			availableKillingBlow = KillingBlowSkillButton;
			HandleButtonIconsForSkill(Skill.KillingBlow, enemyActionType, KillingBlowSkillButton, isEnemyVulnerable);
		}
		else
		{
			KillingBlowSkillButton.gameObject.SetActive(false);
		}

		HandleCanvas(availableSkills, availableKillingBlow);
	}
}
