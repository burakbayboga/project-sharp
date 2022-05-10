using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skewer : Skill
{
    public Skewer()
	{
		Type = SkillType.Skewer;
		BaseCost = new Resource
		{
			Focus = 3,
			Strength = 2,
			Stability = 2
		};

		clip = Animator.StringToHash("counter");
	}

	public override int GetDamageAgainstEnemyAction(SkillType enemyAction)
	{
		return 1;
	}

	public override Resource GetTotalCost(SkillType enemyAction)
	{
		return BaseCost;
	}
}
