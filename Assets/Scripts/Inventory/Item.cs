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
	public ItemType itemType;

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

	public void OnItemClicked()
	{
		GameController.instance.OnItemClicked(itemType);
	}
}
