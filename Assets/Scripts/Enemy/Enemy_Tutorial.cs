using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Tutorial : EnemyType
{
	public override void Init(Enemy _enemy)
	{
		enemy = _enemy;
		enemy.TotalDurability = 2;
	}

	public override SkillType GetActionType()
	{
		if (!enemy.currentHex.IsAdjacentToPlayer())
		{
			return SkillType.None;
		}

		if (enemy.IsVulnerable)
		{
			return SkillType.Block;
		}
		else
		{
			return SkillType.SwiftAttack;
		}
	}

	public override void MoveTurn()
	{
		if (!enemy.currentHex.IsAdjacentToPlayer())
		{
			Hex newHex = enemy.GetHexCloserToPlayer();
			if (newHex != null)
			{
				enemy.MoveToHex(newHex);
			}
		}
	}
}
