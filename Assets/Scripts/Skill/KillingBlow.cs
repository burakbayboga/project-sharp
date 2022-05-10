﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillingBlow : Skill
{
    public KillingBlow()
    {
        Type = SkillType.KillingBlow;
        BaseCost = new Resource
        {
            Focus = 1,
            Strength = 1,
            Stability = 1
        };

		clip = Animator.StringToHash("killing blow");
    }

    public override Resource GetTotalCost(SkillType enemyAction)
    {
		Resource totalCost;
		Resource itemModifier = GetItemModifier();
		if (enemyAction == SkillType.ShootArrow)
		{
			totalCost =  new Resource();
		}
		else if (GameController.instance.IsCurrentEnemyVulnerable())
        {
			if (enemyAction == SkillType.Block)
			{
				totalCost =  BaseCost + itemModifier;
			}
			else
			{
				totalCost =  BaseCost + 1 + itemModifier;
			}
        }
        else
        {
            Resource modifier = new Resource
            {
                Focus = 7,
                Strength = 4,
                Stability = 4
            };

			totalCost = BaseCost + modifier + itemModifier;
        }

		totalCost.Clamp();
		return totalCost;
    }
}
