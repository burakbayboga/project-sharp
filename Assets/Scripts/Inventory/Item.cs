using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Resource resourceModifier;
	public SkillType modifiedSkillType;
	public SkillType unlockedSkillType;
	public ItemType itemType;

	public GameObject description;
	public GameObject shortDescription;

	public bool inInventory;

	public void OnItemClicked()
	{
		if (!inInventory)
		{
			GameController.instance.OnItemClicked(this);
		}
	}
}
