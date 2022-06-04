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
		if (enemy.HasLosToPlayer(enemy.currentHex) && Vector3.Distance(enemy.currentHex.transform.position, Player.instance.currentHex.transform.position) <= 2.2f)
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

	Hex GetSkewerableHex()
	{
		for (int i = 0; i < enemy.currentHex.adjacents.Length; i++)
		{
			Hex hex = enemy.currentHex.adjacents[i];
			if (!hex.isOccupied && enemy.HasLosToPlayer(hex) && Vector3.Distance(hex.transform.position, Player.instance.currentHex.transform.position) <= 2f)
			{
				return enemy.currentHex.adjacents[i];
			}
		}

		return null;
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
			Hex newHex = GetSkewerableHex();
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
