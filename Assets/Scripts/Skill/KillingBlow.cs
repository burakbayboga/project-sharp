using System.Collections;
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
		
		if (GameController.instance.IsCurrentEnemyVulnerable())
        {
			totalCost = BaseCost + itemModifier;
			if (enemyAction == SkillType.Counter)
			{
				totalCost += 1;
			}
        }
        else
        {
			if (enemyAction == SkillType.None)
			{
				Resource modifier = new Resource
				{
					Focus = 3,
					Strength = 2,
					Stability = 2
				};
				totalCost = BaseCost + modifier + itemModifier;
			}
			else
			{
				Resource modifier = new Resource
				{
					Focus = 4,
					Strength = 3,
					Stability = 3
				};
				totalCost = BaseCost + modifier + itemModifier;
			}
        }

		totalCost.Clamp();
		return totalCost;
    }
}
