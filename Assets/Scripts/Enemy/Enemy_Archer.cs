using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Archer : EnemyType
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
			if (enemy.HasLosToPlayer(enemy.currentHex))
			{
				return SkillType.ShootArrow;
			}
			else
			{
				return SkillType.None;
			}
		}

		if (enemy.IsVulnerable)
		{
			return SkillType.Counter;
		}
		else
		{
			return SkillType.SwiftAttack;
		}
	}

	public override void MoveTurn()
	{
		if (enemy.currentHex.IsAdjacentToPlayer() && Random.Range(0f, 1f) < 0.15f)
		{
			// try to get away sometimes
			Hex newHex = enemy.GetHexFurtherToPlayer(true);
			if (newHex != null)
			{
				enemy.MoveToHex(newHex);
			}
		}
		else if (Vector3.Distance(enemy.currentHex.transform.position, Player.instance.currentHex.transform.position) > 3f)
		{
			// too far, try to get closer with los
			Hex newHex = enemy.GetHexCloserToPlayer(true);
			if (newHex != null)
			{
				enemy.MoveToHex(newHex);
			}
			else
			{
				newHex = enemy.GetHexCloserToPlayer(false, true);
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
		else if (!enemy.HasLosToPlayer(enemy.currentHex))
		{
			// try to gain los
			Hex newHex = enemy.GetHexWithLosToPlayer();
			if (newHex != null)
			{
				enemy.MoveToHex(newHex);
			}
			else
			{
				newHex = enemy.GetHexCloserToPlayer(false, true);
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
}
