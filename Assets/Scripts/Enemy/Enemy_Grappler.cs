using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Grappler : EnemyType
{
	public override void Init(Enemy _enemy)
	{
		enemy = _enemy;
		enemy.TotalDurability = 4;
	}

	public override SkillType GetActionType()
	{
		if (!enemy.currentHex.IsAdjacentToPlayer())
		{
			return SkillType.None;
		}
		else
		{
			enemy.isGrappling = true;
			return SkillType.Grapple;
		}
	}

	public override void MoveTurn()
	{
		if (enemy.currentHex.IsAdjacentToPlayer())
		{
			return;
		}

		Hex newHex = enemy.GetHexCloserToPlayer(false, true);
		if (newHex != null)
		{
			enemy.MoveToHex(newHex);
		}
		else
		{
			// PATHFIND
			newHex = AStar.GetHexFirstInPath(enemy.currentHex, Player.instance.currentHex);
			if (newHex != null && !newHex.isOccupied)
			{
				enemy.MoveToHex(newHex);
			}
		}
	}
}
