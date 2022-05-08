using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
	public int focusModifier;
	public int strengthModifier;
	public int stabilityModifier;

    public Resource resourceModifier;
	public SkillType modifiedSkillType;

	public GameObject description;

	void Awake()
	{
		resourceModifier = new Resource
		{
			Focus = focusModifier,
			Strength = strengthModifier,
			Stability = stabilityModifier
		};
	}
}
