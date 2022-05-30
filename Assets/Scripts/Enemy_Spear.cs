using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spear : EnemyType
{
	public override void Init(Enemy _enemy)
	{
		enemy = _enemy;
		enemy.TotalDurability = 3;
	}

	public override SkillType GetActionType()
	{
		if (enemy.HasLosToPlayer(enemy.currentHex) && Vector3.Distance(enemy.currentHex.transform.position, Player.instance.currentHex.transform.position) <= 2f)
		{
			if (enemy.currentHex.IsAdjacentToPlayer() && enemy.IsVulnerable)
			{
				return SkillType.Counter;
			}
			else
			{
				return SkillType.Skewer;
			}
		}
		else
		{
			return SkillType.None;
		}
	}

	public override void MoveTurn()
	{
		if (enemy.currentHex.IsAdjacentToPlayer() && Random.Range(0f, 1f) < 0.25f)
		{
			// try to get away sometimes
			Hex newHex = enemy.GetHexFurtherToPlayer(true);
			if (newHex != null)
			{
				enemy.MoveToHex(newHex);
			}
		}
		else
		{
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
}
