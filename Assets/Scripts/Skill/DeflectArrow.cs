using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeflectArrow : Skill
{
	public DeflectArrow()
	{
		Type = SkillType.DeflectArrow;
		BaseCost = new Resource
		{
			Focus = 2,
			Strength = 0,
			Stability = 0
		};
	}
	
	public override Resource GetTotalCost(SkillType enemyAction)
	{
		Resource modifier;
		Resource totalCost;

		switch (enemyAction)
		{
			case SkillType.ShootArrow:
				modifier = new Resource
				{
					Focus = 0,
					Strength = 0,
					Stability = 0
				};
				totalCost = BaseCost + modifier;

				break;
			default:
				totalCost = new Resource();

				break;
		}

		return totalCost;
	}
}
