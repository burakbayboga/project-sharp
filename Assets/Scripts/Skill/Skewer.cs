using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skewer : Skill
{
    public Skewer()
	{
		Type = SkillType.Skewer;
		BaseCost = new Resource
		{
			Focus = 2,
			Strength = 3,
			Stability = 2
		};

		clip = Animator.StringToHash("counter");
	}

	public override int GetDamageAgainstEnemyAction(Skill enemyAction)
	{
		return 1;
	}

	public override Resource GetTotalCost(SkillType enemyAction)
	{
		Resource itemModifier = GetItemModifier();
		Resource totalCost = BaseCost + itemModifier;
		totalCost.Clamp();
		return totalCost;
	}
}
