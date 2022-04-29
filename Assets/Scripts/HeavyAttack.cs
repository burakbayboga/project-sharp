using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyAttack : Skill
{
    public HeavyAttack()
    {
        Type = SkillType.HeavyAttack;
        BaseCost = new Resource
        {
            Focus = 1,
            Strength = 3,
            Stability = 2
        };
    }

    public override void HandleClash(Enemy enemy, SkillType playerReaction)
    {
        switch (playerReaction)
        {
            case SkillType.SwiftAttack:
                enemy.ExposeWeakness(1);
                break;
            case SkillType.HeavyAttack:
            case SkillType.Block:
                enemy.ExposeWeakness(2);
                break;
            case SkillType.Counter:
                enemy.ExposeWeakness(1);
                break;
            case SkillType.KillingBlow:
                GameController.instance.MarkEnemyForDeath(enemy);
                break;
            case SkillType.None:
                Player.instance.GetInjury();
                break;
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
                    Strength = 1,
                    Stability = 1
                };
                totalCost = BaseCost + modifier;

                damage = 2;
                break;
            case SkillType.SwiftAttack:

                modifier = new Resource
                {
                    Focus = 1,
                    Strength = 0,
                    Stability = 0
                };
                totalCost = BaseCost + modifier;

                damage = 2;
                break;
            case SkillType.Block:

                modifier = new Resource
                {
                    Focus = -2,
                    Strength = 0,
                    Stability = 1
                };
                totalCost = BaseCost + modifier;

                damage = 2;
                break;
            case SkillType.Counter:

                modifier = new Resource
                {
                    Focus = 3,
                    Strength = 0,
                    Stability = 1
                };
                totalCost = BaseCost + modifier;

                damage = 2;
                break;
            default:
                totalCost = new Resource();
                damage = 0;
                break;
        }

        return totalCost;
    }
}