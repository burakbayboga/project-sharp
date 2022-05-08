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

	public bool inInventory;

	void Awake()
	{
		resourceModifier = new Resource
		{
			Focus = focusModifier,
			Strength = strengthModifier,
			Stability = stabilityModifier
		};
	}

	public void OnMouseEnter()
	{
		if (inInventory)
		{
			description.SetActive(true);
		}
	}

	public void OnMouseExit()
	{
		if (inInventory)
		{
			description.SetActive(false);
		}
	}

	public void OnItemClicked()
	{
		if (!inInventory)
		{
			GameController.instance.OnItemClicked(itemType);
		}
	}
}
