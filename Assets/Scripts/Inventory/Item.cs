using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Resource resourceModifier;
	public List<SkillType> modifiedSkillTypes;
	public SkillType unlockedSkillType;
	public ItemType itemType;
	public ItemRarity itemRarity;
	public int armorGiven;

	public bool inInventory;

	public void OnItemClicked()
	{
		if (!inInventory)
		{
			GameController.instance.OnItemClicked(this);
		}
	}
}

public enum ItemType
{
	skillModifier = 0,
	resourceBooster = 1,
	unlocksSkill = 2,
	armor = 3,
	rechargeBooster = 4,
	infiniteResourcePowerup = 5
}

public enum ItemRarity
{
	common = 0,
	rare = 1,
	epic = 2
}
