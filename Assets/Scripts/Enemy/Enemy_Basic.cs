using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : EnemyType
{
	public override void Init(Enemy _enemy)
	{
		enemy = _enemy;
		enemy.TotalDurability = 3;
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
		else if (enemy.CurrentWeaknessExposed > 2)
		{
			if (Random.Range(0f, 1f) < 0.4f)
			{
				return SkillType.Block;
			}
			else
			{
				return SkillType.SwiftAttack;
			}
		}
		else
		{
			if (Random.Range(0f, 1f) < 0.8f)
			{
				return SkillType.SwiftAttack;
			}
			else
			{
				return SkillType.HeavyAttack;
			}
		}
	}

	public override void MoveTurn()
	{
		if (enemy.currentHex.IsAdjacentToPlayer())
		{
			if (enemy.IsVulnerable && Random.Range(0f, 1f) < 0.15f)
			{
				Hex newHex = enemy.GetHexFurtherToPlayer();
				if (newHex != null)
				{
					enemy.MoveToHex(newHex);
				}
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
