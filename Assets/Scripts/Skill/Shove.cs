using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shove : Skill
{
	public Shove()
	{
		Type = SkillType.Shove;
		BaseCost = new Resource()
		{
			Focus = 0,
			Strength = 2,
			Stability = 2
		};

		clip = Animator.StringToHash("counter");
	}

	public override Resource GetTotalCost(SkillType enemyAction)
	{
		Resource itemModifier = GetItemModifier();
		Resource totalCost = BaseCost + itemModifier;
		totalCost.Clamp();
		return totalCost;
	}
}
