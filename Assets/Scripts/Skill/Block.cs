﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : Skill
{
    public Block()
    {
        Type = SkillType.Block;
        BaseCost = new Resource
        {
            Focus = 0,
            Strength = 2,
            Stability = 2
        };

		clip = Animator.StringToHash("block");
    }

	public override int GetCoveredWeaknessByEnemy()
	{
		return 2;
	}

	public override int GetDamageAgainstEnemyAction(Skill enemyAction)
	{
		SkillType enemyActionType = enemyAction != null ? enemyAction.Type : SkillType.None;
		switch (enemyActionType)
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
		Resource itemModifier = GetItemModifier();

        switch (enemyAction)
        {
            case SkillType.HeavyAttack:

                modifier = new Resource
                {
                    Focus = 0,
                    Strength = 0,
                    Stability = 2
                };
                totalCost = BaseCost + modifier + itemModifier;

                break;
            case SkillType.SwiftAttack:

                modifier = new Resource
                {
                    Focus = 0,
                    Strength = -1,
                    Stability = 0
                };
                totalCost = BaseCost + modifier + itemModifier;

                break;
            case SkillType.Block:
            case SkillType.Counter:
            default:
                totalCost = new Resource();
                break;
        }

		totalCost.Clamp();
        return totalCost;
    }
}
