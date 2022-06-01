using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charge : Skill
{
	public Charge()
	{
		Type = SkillType.Charge;
		BaseCost = new Resource()
		{
			Focus = 1,
			Strength = 2,
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
