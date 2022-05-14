using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlwind : Skill
{
	public Whirlwind()
	{
		Type = SkillType.Whirlwind;
		BaseCost = new Resource()
		{
			Focus = 4,
			Strength = 2,
			Stability = 3
		};

		clip = Animator.StringToHash("counter");
	}

	public override int GetDamageAgainstEnemyAction(SkillType enemyAction)
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
