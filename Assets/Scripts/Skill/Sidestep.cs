using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sidestep : Skill
{
	public Sidestep()
	{
		Type = SkillType.Sidestep;
		BaseCost = new Resource
		{
			Focus = 2,
			Strength = 0,
			Stability = 1
		};
	}

	public override Resource GetTotalCost(SkillType enemyAction)
	{
		Resource itemModifier = GetItemModifier();
		Resource totalCost = BaseCost + itemModifier;
		totalCost.Clamp();
		return totalCost;
	}
}
