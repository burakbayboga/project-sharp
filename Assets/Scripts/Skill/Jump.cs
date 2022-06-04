using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : Skill
{
	public Jump()
	{
		Type = SkillType.Jump;
		BaseCost = new Resource
		{
			Focus = 2,
			Strength = 1,
			Stability = 2
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
