using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heartshot : Skill
{
    public Heartshot()
    {
        Type = SkillType.Heartshot;
        BaseCost = new Resource
        {
            Focus = 2,
            Strength = 0,
            Stability = 1
        };

		clip = Animator.StringToHash("shoot arrow");
    }

    public override Resource GetTotalCost(SkillType enemyAction)
    {
		Resource totalCost;
		Resource itemModifier = GetItemModifier();
		
		if (GameController.instance.IsCurrentEnemyVulnerable())
        {
			totalCost = BaseCost + itemModifier;
        }
        else
        {
			if (enemyAction == SkillType.None)
			{
				Resource modifier = new Resource
				{
					Focus = 3,
					Strength = 0,
					Stability = 2
				};
				totalCost = BaseCost + modifier + itemModifier;
			}
			else
			{
				Resource modifier = new Resource
				{
					Focus = 4,
					Strength = 0,
					Stability = 3
				};
				totalCost = BaseCost + modifier + itemModifier;
			}
        }

		totalCost.Clamp();
		return totalCost;
    }
}
