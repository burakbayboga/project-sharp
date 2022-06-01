using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Brute : EnemyType
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

		if (enemy.IsVulnerable)
		{
			if (Random.Range(0f, 1f) < 0.6f)
			{
				return SkillType.HeavyAttack;
			}
			else
			{
				return SkillType.Block;
			}
		}
		else
		{
			if (Random.Range(0f, 1f) < 0.8f)
			{
				return SkillType.HeavyAttack;
			}
			else
			{
				return SkillType.SwiftAttack;
			}
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
