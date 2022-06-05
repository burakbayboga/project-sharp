using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : Skill
{
	public Grapple()
	{
		Type = SkillType.Grapple;
		BaseCost = new Resource
		{
			Focus = 2,
			Strength = 0,
			Stability = 1
		};

		clip = Animator.StringToHash("counter");
	}
}
