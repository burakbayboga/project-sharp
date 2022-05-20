using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Archer : Enemy
{
	public override void Init(Hex spawnHex)
	{
		TotalDurability = 2;
		rend.color = color;
		base.Init(spawnHex);
	}

	protected override SkillType GetActionType()
	{
		if (!currentHex.IsAdjacentToPlayer())
		{
			if (HasLosToPlayer(currentHex))
			{
				return SkillType.ShootArrow;
			}
			else
			{
				return SkillType.None;
			}
		}

		if (IsVulnerable)
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
		if (currentHex.IsAdjacentToPlayer() && Random.Range(0f, 1f) < 0.15f)
		{
			// try to get away sometimes
			Hex newHex = GetHexFurtherToPlayer(true);
			if (newHex != null)
			{
				MoveToHex(newHex);
			}
		}
		else if (Vector3.Distance(currentHex.transform.position, Player.instance.currentHex.transform.position) > 3f)
		{
			// too far, try to get closer with los
			Hex newHex = GetHexCloserToPlayer(true);
			if (newHex != null)
			{
				MoveToHex(newHex);
			}
			else
			{
				newHex = GetHexCloserToPlayer(false);
				if (newHex != null)
				{
					MoveToHex(newHex);
				}
			}
		}
		else if (!HasLosToPlayer(currentHex))
		{
			// try to gain los
			Hex newHex = GetHexWithLosToPlayer();
			if (newHex != null)
			{
				MoveToHex(newHex);
			}
			else
			{
				newHex = GetHexCloserToPlayer();
				if (newHex != null)
				{
					MoveToHex(newHex);
				}
			}
		}
	}
}
