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
            Focus = 2,
            Strength = 0,
            Stability = 2
        };

		clip = Animator.StringToHash("counter");
    }

	public override int GetCoveredWeaknessByEnemy()
	{
		return 1;
	}
    
	public override int GetDamageAgainstEnemyAction(Skill enemyAction)
	{
		SkillType enemyActionType = enemyAction != null ? enemyAction.Type : SkillType.None;
		switch (enemyActionType)
		{
			case SkillType.HeavyAttack:
				return 1;
			case SkillType.SwiftAttack:
			case SkillType.Skewer:
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
                    Focus = 0,
                    Strength = 0,
                    Stability = 0
                };
                break;
            case SkillType.SwiftAttack:
                modifier = new Resource
                {
                    Focus = 1,
                    Strength = 0,
                    Stability = 0
                };
                break;
			case SkillType.Skewer:
				modifier = new Resource
				{
					Focus = 2,
					Strength = 0,
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
