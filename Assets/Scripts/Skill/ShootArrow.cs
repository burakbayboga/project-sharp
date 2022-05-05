using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootArrow : Skill
{
	public ShootArrow()
	{
		Type = SkillType.ShootArrow;
		BaseCost = new Resource
		{
			Focus = 2,
			Strength = 0,
			Stability = 1
		};

		clip = Animator.StringToHash("shoot arrow");
	}
}
