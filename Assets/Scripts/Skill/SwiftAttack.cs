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

		clip = Animator.StringToHash("swift attack");
    }

	public override int GetDamageAgainstEnemyAction(Skill enemyAction)
	{
		SkillType enemyActionType = enemyAction != null ? enemyAction.Type : SkillType.None;
		switch (enemyActionType)
		{
			case SkillType.HeavyAttack:
			case SkillType.SwiftAttack:
			case SkillType.Block:
			case SkillType.Counter:
				return 1;
			case SkillType.None:
				return 2;
			default:
				return 0;
		}
	}

    public override Resource GetTotalCost(SkillType enemyAction)
    {
        Resource modifier;
        Resource totalCost;
		Resource itemModifier = GetItemModifier();

        switch (enemyAction)
        {
            case SkillType.HeavyAttack:

                modifier = new Resource
                {
                    Focus = 1,
                    Strength = 1,
                    Stability = 0
                };
                totalCost = BaseCost + modifier + itemModifier;

                break;
            case SkillType.SwiftAttack:

                modifier = new Resource
                {
                    Focus = 2,
                    Strength = -1,
                    Stability = 0
                };
                totalCost = BaseCost + modifier + itemModifier;

                break;
            case SkillType.Block:

                modifier = new Resource
                {
                    Focus = 0,
                    Strength = 0,
                    Stability = 0
                };
                totalCost = BaseCost + modifier + itemModifier;

                break;
            case SkillType.Counter:

                modifier = new Resource
                {
                    Focus = 2,
                    Strength = 0,
                    Stability = 1
                };
                totalCost = BaseCost + modifier + itemModifier;

                break;
			case SkillType.None:

				modifier = new Resource()
				{
					Focus = -1,
					Strength = -1,
					Stability = -1
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
