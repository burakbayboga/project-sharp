using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCanvas : MonoBehaviour
{
	public static SkillCanvas instance;

    public SkillButton SwiftAttackSkillButton;
    public SkillButton HeavyAttackSkillButton;
    public SkillButton BlockSkillButton;
    public SkillButton CounterSkillButton;
    public SkillButton KillingBlowSkillButton;
	public SkillButton DeflectArrowSkillButton;
	public SkillButton SkewerSkillButton;
	public SkillButton BlockArrowSkillButton;
	public SkillButton WhirlwindSkillButton;
	public SkillButton HookSkillButton;
	public SkillButton WrestleSkillButton;
	public SkillButton ShoveSkillButton;
	public SkillButton HeartshotSkillButton;
	public SkillButton LightningReflexesSkillButton;
	public SkillButton ChargeSkillButton;


	void Awake()
	{
		instance = this;
		gameObject.SetActive(false);
	}

	public void HandleSkills(bool isEnemyShootingArrow, bool isEnemySkewering, bool isEnemyDefensive, bool isEnemyVulnerable, bool isAdjacentToEnemy, bool isEnemyIdle, bool hasLos, bool wrestleUsed, bool canSkewer, bool chargeUsed, bool gap, SkillType enemyActionType, bool isEnemyAnswered, bool isEnemyGrappling)
	{
		List<SkillButton> availableSkills = new List<SkillButton>();
		SkillButton availableKillingBlow = null;

		if (!isEnemyAnswered && !isEnemyGrappling && ((!isEnemyShootingArrow && !isEnemyDefensive && isAdjacentToEnemy && !isEnemyIdle) || isEnemySkewering))
		{
			BlockSkillButton.gameObject.SetActive(true);
			availableSkills.Add(BlockSkillButton);
			HandleButtonIconsForSkill(Skill.Block, enemyActionType, BlockSkillButton);
		}
		else
		{
			BlockSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && !isEnemyGrappling && ((!isEnemyShootingArrow && !isEnemyDefensive && isAdjacentToEnemy && !isEnemyIdle) || isEnemySkewering))
		{
			CounterSkillButton.gameObject.SetActive(true);
			availableSkills.Add(CounterSkillButton);
			HandleButtonIconsForSkill(Skill.Counter, enemyActionType, CounterSkillButton);
		}
		else
		{
			CounterSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && !isEnemyGrappling && !isEnemyVulnerable && isAdjacentToEnemy)
		{
			SwiftAttackSkillButton.gameObject.SetActive(true);
			availableSkills.Add(SwiftAttackSkillButton);
			HandleButtonIconsForSkill(Skill.SwiftAttack, enemyActionType, SwiftAttackSkillButton);
		}
		else
		{
			SwiftAttackSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && !isEnemyShootingArrow && !isEnemyVulnerable && isAdjacentToEnemy)
		{
			HeavyAttackSkillButton.gameObject.SetActive(true);
			availableSkills.Add(HeavyAttackSkillButton);
			HandleButtonIconsForSkill(Skill.HeavyAttack, enemyActionType, HeavyAttackSkillButton);
		}
		else
		{
			HeavyAttackSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && GameController.instance.isSkewerUnlocked && canSkewer)
		{
			SkewerSkillButton.gameObject.SetActive(true);
			availableSkills.Add(SkewerSkillButton);
			HandleButtonIconsForSkill(Skill.Skewer, enemyActionType, SkewerSkillButton);
		}
		else
		{
			SkewerSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && isEnemyShootingArrow)
		{
			DeflectArrowSkillButton.gameObject.SetActive(true);
			availableSkills.Add(DeflectArrowSkillButton);
			HandleButtonIconsForSkill(Skill.DeflectArrow, enemyActionType, DeflectArrowSkillButton);
		}
		else
		{
			DeflectArrowSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && GameController.instance.isLightningReflexesUnlocked && isEnemyShootingArrow)
		{
			LightningReflexesSkillButton.gameObject.SetActive(true);
			availableSkills.Add(LightningReflexesSkillButton);
			HandleButtonIconsForSkill(Skill.LightningReflexes, enemyActionType, LightningReflexesSkillButton);
		}
		else
		{
			LightningReflexesSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && GameController.instance.isBlockArrowUnlocked && isEnemyShootingArrow)
		{
			BlockArrowSkillButton.gameObject.SetActive(true);
			availableSkills.Add(BlockArrowSkillButton);
			HandleButtonIconsForSkill(Skill.BlockArrow, enemyActionType, BlockArrowSkillButton);
		}
		else
		{
			BlockArrowSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && GameController.instance.isWhirlwindUnlocked && isAdjacentToEnemy)
		{
			WhirlwindSkillButton.gameObject.SetActive(true);
			availableSkills.Add(WhirlwindSkillButton);
			HandleButtonIconsForSkill(Skill.Whirlwind, enemyActionType, WhirlwindSkillButton);
		}
		else
		{
			WhirlwindSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && GameController.instance.isHookUnlocked && !isAdjacentToEnemy && hasLos)
		{
			HookSkillButton.gameObject.SetActive(true);
			availableSkills.Add(HookSkillButton);
			HandleButtonIconsForSkill(Skill.Hook, enemyActionType, HookSkillButton);
		}
		else
		{
			HookSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && isAdjacentToEnemy)
		{
			ShoveSkillButton.gameObject.SetActive(true);
			availableSkills.Add(ShoveSkillButton);
			HandleButtonIconsForSkill(Skill.Shove, enemyActionType, ShoveSkillButton);
		}
		else
		{
			ShoveSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && isAdjacentToEnemy)
		{
			KillingBlowSkillButton.gameObject.SetActive(true);
			availableKillingBlow = KillingBlowSkillButton;
			HandleButtonIconsForSkill(Skill.KillingBlow, enemyActionType, KillingBlowSkillButton, isEnemyVulnerable);
		}
		else
		{
			KillingBlowSkillButton.gameObject.SetActive(false);
		}
		if (!isEnemyAnswered && GameController.instance.isHeartshotUnlocked && !isAdjacentToEnemy && hasLos)
		{
			HeartshotSkillButton.gameObject.SetActive(true);
			availableKillingBlow = HeartshotSkillButton;
			HandleButtonIconsForSkill(Skill.Heartshot, enemyActionType, HeartshotSkillButton, isEnemyVulnerable);
		}
		else
		{
			HeartshotSkillButton.gameObject.SetActive(false);
		}
		if (GameController.instance.isWrestleUnlocked && isAdjacentToEnemy && !wrestleUsed && !isEnemyGrappling)
		{
			WrestleSkillButton.gameObject.SetActive(true);
			availableSkills.Add(WrestleSkillButton);
			HandleButtonIconsForSkill(Skill.Wrestle, enemyActionType, WrestleSkillButton);
		}
		else
		{
			WrestleSkillButton.gameObject.SetActive(false);
		}
		if (!isAdjacentToEnemy && GameController.instance.isChargeUnlocked && !chargeUsed && hasLos && !gap)
		{
			ChargeSkillButton.gameObject.SetActive(true);
			availableSkills.Add(ChargeSkillButton);
			HandleButtonIconsForSkill(Skill.Charge, enemyActionType, ChargeSkillButton);
		}
		else
		{
			ChargeSkillButton.gameObject.SetActive(false);
		}

		HandleCanvas(availableSkills, availableKillingBlow);
	}

	protected void HandleCanvas(List<SkillButton> availableSkills, SkillButton availableKillingBlow)
	{
		float radius = 1.7f;
		float increment = 30f * Mathf.Deg2Rad;
		float theta = 90f * Mathf.Deg2Rad;
		int i = 0;

		Vector3 pos;
		for (; i < availableSkills.Count; i++)
		{
			pos = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f) * radius;
			availableSkills[i].SetPosition(pos);
			//if (i == 6)
			//{
				//break;
			//}
			theta -= increment;
		}
		if (availableKillingBlow != null)
		{
			//float angle = 90f * Mathf.Deg2Rad + increment;
			//availableKillingBlow.SetPosition(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
			availableKillingBlow.SetPosition(new Vector3(-radius, 0f, 0f));
		}

		theta = 90f * Mathf.Deg2Rad - increment / 2f;
		radius *= 1.7f;
		for (; i < availableSkills.Count; i++)
		{
			pos = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f) * radius;
			availableSkills[i].SetPosition(pos);
			theta -= increment;
		}
	}

	public static void HandleButtonIconsForSkill(Skill skill, SkillType enemyActionType, SkillButton skillButton, bool setKillingBlowIndicator = false)
	{
		List<Enemy> adjacentEnemies = Player.instance.currentHex.GetAdjacentEnemies();
		bool beingGrappled = false;
		for (int i = 0; i < adjacentEnemies.Count; i++)
		{
			if (adjacentEnemies[i].isGrappling)
			{
				beingGrappled = true;
			}
		}
		Resource cost = skill.GetTotalCost(enemyActionType);
		int damage = skill.GetDamageAgainstEnemyAction(Skill.GetSkillForType(enemyActionType));
		skillButton.HandleCostAndDamage(cost, damage, setKillingBlowIndicator, beingGrappled);
	}
}
