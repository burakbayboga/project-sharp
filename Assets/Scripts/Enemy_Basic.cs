using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : Enemy
{
	public override void Init(Hex spawnHex)
	{
		TotalDurability = 3;
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
		else if (CurrentWeaknessExposed > 2)
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
		if (currentHex.IsAdjacentToPlayer())
		{
			if (IsVulnerable && Random.Range(0f, 1f) < 0.25f)
			{
				Hex newHex = GetHexFurtherToPlayer();
				if (newHex != null)
				{
					MoveToHex(newHex);
				}
			}
		}
		else
		{
			Hex newHex = GetHexCloserToPlayer();
			if (newHex != null)
			{
				MoveToHex(newHex);
			}
		}
	}
}
