using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Brute : Enemy
{
	public override void Init(Hex spawnHex)
	{
		TotalDurability = 4;
		rend.color = color;
		base.Init(spawnHex);
	}

	protected override SkillType GetActionType()
	{
		if (!currentHex.IsAdjacentToPlayer())
		{
			return SkillType.None;
		}

		if (IsVulnerable)
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
		if (currentHex.IsAdjacentToPlayer())
		{
			return;
		}

		Hex newHex = GetHexCloserToPlayer();
		if (newHex != null)
		{
			MoveToHex(newHex);
		}
	}
}
