using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockArrow : Skill
{
	public BlockArrow()
	{
		Type = SkillType.BlockArrow;
		BaseCost = new Resource
		{
			Focus = 0,
			Strength = 0,
			Stability = 2
		};

		clip = Animator.StringToHash("block");
	}
	
	public override Resource GetTotalCost(SkillType enemyAction)
	{
		Resource modifier;
		Resource totalCost;
		Resource itemModifier = GetItemModifier();

		switch (enemyAction)
		{
			case SkillType.ShootArrow:
				modifier = new Resource
				{
					Focus = 0,
					Strength = 0,
					Stability = 0
				};
				totalCost = BaseCost + modifier + itemModifier;

				break;
			default:
				totalCost = new Resource();

				break;
		}

		totalCost.Clamp();
		return totalCost;
	}
}
