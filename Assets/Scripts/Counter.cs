using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : Skill
{
    public Counter()
    {
        Type = SkillType.Counter;
        BaseCost = new Resource
        {
            Focus = 3,
            Strength = 1,
            Stability = 2
        };
    }

    public override void HandleClash(Enemy enemy, SkillType playerReaction)
    {
        switch (playerReaction)
        {
            case SkillType.HeavyAttack:
                enemy.ExposeWeakness(2);
                break;
            case SkillType.KillingBlow:
                GameController.instance.MarkEnemyForDeath(enemy);
                break;
            case SkillType.SwiftAttack:
                enemy.ExposeWeakness(1);
                break;
            case SkillType.Block:
            case SkillType.Counter:
            case SkillType.None:
            default:
                break;
        }
    }

    public override Resource GetTotalCost(SkillType enemyAction, out int damage)
    {
        Resource modifier;
        Resource totalCost;

        switch (enemyAction)
        {
            case SkillType.HeavyAttack:

                modifier = new Resource
                {
                    Focus = -1,
                    Strength = 0,
                    Stability = 1
                };
                totalCost = BaseCost + modifier;

                damage = 1;
                break;
            case SkillType.SwiftAttack:

                modifier = new Resource
                {
                    Focus = 1,
                    Strength = -1,
                    Stability = 1
                };
                totalCost = BaseCost + modifier;

                damage = 2;
                break;
            case SkillType.Block:
            case SkillType.Counter:
            default:
                totalCost = new Resource();
                damage = 0;
                break;
        }

        return totalCost;
    }
}
