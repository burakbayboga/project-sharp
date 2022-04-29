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
    }

    public override void HandleClash(Enemy enemy, SkillType playerReaction)
    {

    }

    public override Resource GetTotalCost(SkillType enemyAction, out int damage)
    {
        damage = 0;

        if (GameController.instance.IsCurrentEnemyVulnerable())
        {
            return BaseCost;
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
