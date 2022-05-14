using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlwind : Skill
{
	public Whirlwind()
	{
		Type = SkillType.Whirlwind;
		BaseCost = new Resource()
		{
			Focus = 2,
			Strength = 2,
			Stability = 3
		};

		clip = Animator.StringToHash("counter");
	}

	public override int GetDamageAgainstEnemyAction(SkillType enemyAction)
	{
		return 1;
	}
	
	public override Resource GetTotalCost(SkillType enemyAction)
	{
		Resource itemModifier = GetItemModifier();
		Resource enemyCountModifier = new Resource();
		enemyCountModifier.Focus = Player.instance.currentHex.GetAdjacentEnemyCount();
		Resource totalCost = BaseCost + itemModifier + enemyCountModifier;
		totalCost.Clamp();
		return totalCost;
	}
}
