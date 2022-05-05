using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwiftAttack : Skill
{
    public SwiftAttack()
    {
        Type = SkillType.SwiftAttack;
        BaseCost = new Resource
        {
            Focus = 1,
            Strength = 2,
            Stability = 1
        };
    }

	public override int GetDamageAgainstEnemyAction(SkillType enemyAction)
	{
		switch (enemyAction)
		{
			case SkillType.HeavyAttack:
			case SkillType.SwiftAttack:
			case SkillType.Block:
			case SkillType.Counter:
				return 1;
			default:
				return 0;
		}
	}

    public override Resource GetTotalCost(SkillType enemyAction)
    {
        Resource modifier;
        Resource totalCost;

        switch (enemyAction)
        {
            case SkillType.HeavyAttack:

                modifier = new Resource
                {
                    Focus = 1,
                    Strength = 1,
                    Stability = 0
                };
                totalCost = BaseCost + modifier;

                break;
            case SkillType.SwiftAttack:

                modifier = new Resource
                {
                    Focus = 2,
                    Strength = -1,
                    Stability = 0
                };
                totalCost = BaseCost + modifier;

                break;
            case SkillType.Block:

                modifier = new Resource
                {
                    Focus = 0,
                    Strength = 0,
                    Stability = 0
                };
                totalCost = BaseCost + modifier;

                break;
            case SkillType.Counter:

                modifier = new Resource
                {
                    Focus = 2,
                    Strength = 0,
                    Stability = 1
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
