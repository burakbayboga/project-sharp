using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Tutorial : Enemy
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
			return SkillType.None;
		}

		if (IsVulnerable)
		{
			return SkillType.Block;
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
		if (!currentHex.IsAdjacentToPlayer())
		{
			Hex newHex = GetHexCloserToPlayer();
			if (newHex != null)
			{
				MoveToHex(newHex);
			}
		}
	}
}
