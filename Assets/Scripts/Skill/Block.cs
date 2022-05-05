using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : Skill
{
    public Block()
    {
        Type = SkillType.Block;
        BaseCost = new Resource
        {
            Focus = 1,
            Strength = 2,
            Stability = 2
        };

		clip = Animator.StringToHash("block");
    }

	public override int GetDamageAgainstEnemyAction(SkillType enemyAction)
	{
		switch (enemyAction)
		{
			case SkillType.HeavyAttack:
				return 2;
			case SkillType.SwiftAttack:
				return 1;
			case SkillType.Block:
			case SkillType.Counter:
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
                    Focus = -1,
                    Strength = 0,
                    Stability = 2
                };
                totalCost = BaseCost + modifier;

                break;
            case SkillType.SwiftAttack:

                modifier = new Resource
                {
                    Focus = 1,
                    Strength = -1,
                    Stability = 0
                };
                totalCost = BaseCost + modifier;

                break;
            case SkillType.Block:
            case SkillType.Counter:
            default:
                totalCost = new Resource();
                break;
        }

        return totalCost;
    }
}
