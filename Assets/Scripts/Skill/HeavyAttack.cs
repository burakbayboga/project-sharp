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
            Focus = 0,
            Strength = 3,
            Stability = 2
        };

		clip = Animator.StringToHash("heavy attack");
    }

	public override int GetDamageAgainstEnemyAction(Skill enemyAction)
	{
		SkillType enemyActionType = enemyAction != null ? enemyAction.Type : SkillType.None;
		switch (enemyActionType)
		{
			case SkillType.Grapple:
				return 1;
			case SkillType.HeavyAttack:
			case SkillType.SwiftAttack:
			case SkillType.Block:
			case SkillType.Counter:
			case SkillType.Skewer:
				return 2;
			case SkillType.None:
				return 3;
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
                    Focus = 0,
                    Strength = 1,
                    Stability = 0
                };
                break;
            case SkillType.SwiftAttack:
                modifier = new Resource
                {
                    Focus = 0,
                    Strength = -1,
                    Stability = 1
                };
                break;
            case SkillType.Block:
                modifier = new Resource
                {
                    Focus = 0,
                    Strength = -1,
                    Stability = 0
                };
                break;
            case SkillType.Counter:
                modifier = new Resource
                {
                    Focus = 0,
                    Strength = 0,
                    Stability = 1
                };
                break;
			case SkillType.Skewer:
				modifier = new Resource
				{
					Focus = 0,
					Strength = -1,
					Stability = 1
				};
				break;
			case SkillType.None:
				modifier = new Resource
				{
					Focus = 0,
					Strength = -1,
					Stability = -1
				};
				break;
			case SkillType.Grapple:
				modifier = new Resource
				{
					Focus = 0,
					Strength = -1,
					Stability = -1
				};
				break;
            default:
				modifier = new Resource();
                break;
        }
		
		totalCost = BaseCost + modifier + itemModifier;
		totalCost.Clamp();
        return totalCost;
    }
}
