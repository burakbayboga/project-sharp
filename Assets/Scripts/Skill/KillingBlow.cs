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
		if (enemyAction == SkillType.ShootArrow)
		{
			return new Resource();
		}
		else if (GameController.instance.IsCurrentEnemyVulnerable())
        {
			if (enemyAction == SkillType.Block)
			{
				return BaseCost;
			}
			else
			{
				return BaseCost + 1;
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
            return BaseCost + modifier;
        }
    }
}
