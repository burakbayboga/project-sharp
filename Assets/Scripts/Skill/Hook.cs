using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : Skill
{
	public Hook()
	{
		Type = SkillType.Hook;
		BaseCost = new Resource
		{
			Focus = 1,
			Strength = 2,
			Stability = 1
		};
	
		clip = Animator.StringToHash("counter");
	}

	public override int GetDamageAgainstEnemyAction(Skill enemyAction)
	{
		SkillType enemyActionType = enemyAction != null ? enemyAction.Type : SkillType.None;
		switch (enemyActionType)
		{
			case SkillType.ShootArrow:
				return 1;
			case SkillType.None:
				return 2;
			default:
				return 0;
		}
	}

	public override Resource GetTotalCost(SkillType enemyAction)
	{
		Resource itemModifier = GetItemModifier();
		Resource totalCost = BaseCost + itemModifier;
		totalCost.Clamp();
		return totalCost;
	}
}
