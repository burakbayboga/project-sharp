using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wrestle : Skill
{
	public Wrestle()
	{
		Type = SkillType.Wrestle;
		BaseCost = new Resource()
		{
			Focus = 1,
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
