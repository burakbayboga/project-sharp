using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LootPanel : MonoBehaviour
{
	public List<Item> itemPool;
	List<Item> currentlyVisible = new List<Item>();
	public List<Item> commonItems = new List<Item>();
	public List<Item> rareItems = new List<Item>();
	public List<Item> epicItems = new List<Item>();


	public float[] lootChances = new float[3]{ 0.7f, 0.25f, 0.05f };

	bool inited = false;

	public void Fill()
	{
		if (!inited)
		{
			Init();
		}

		commonItems = commonItems.OrderBy(r => Random.Range(0f, 1f)).ToList();
		rareItems = rareItems.OrderBy(r => Random.Range(0f, 1f)).ToList();
		epicItems = epicItems.OrderBy(r => Random.Range(0f, 1f)).ToList();

		int count = Mathf.Min(3, itemPool.Count);

		for (int i = 0; i < count; i++)
		{
			float random = Random.Range(0f, 1f);
			ItemRarity rarity;
			if (random < lootChances[0])
			{
				rarity = ItemRarity.common;
			}
			else if (random < lootChances[0] + lootChances[1])
			{
				rarity = ItemRarity.rare;
			}
			else
			{
				rarity = ItemRarity.epic;
			}

			Item lootableItem = GetItemFromPool(rarity);
			currentlyVisible.Add(lootableItem);
			lootableItem.gameObject.SetActive(true);
		}
	}

	// TODO: bu kodu yazan çocuk kör sağır ve dilsiz oldu
	Item GetItemFromPool(ItemRarity rarity)
	{
		Item item = null;
		switch (rarity)
		{
			case ItemRarity.common:
				if (commonItems.Count > 0)
				{
					item = commonItems[0];
					commonItems.Remove(item);
				}
				else if (rareItems.Count > 0)
				{
					item = rareItems[0];
					rareItems.Remove(item);
				}
				else if (epicItems.Count > 0)
				{
					item = epicItems[0];
					epicItems.Remove(item);
				}
				break;
			case ItemRarity.rare:
				if (rareItems.Count > 0)
				{
					item = rareItems[0];
					rareItems.Remove(item);
				}
				else if (epicItems.Count > 0)
				{
					item = epicItems[0];
					epicItems.Remove(item);
				}
				else if (commonItems.Count > 0)
				{
					item = commonItems[0];
					commonItems.Remove(item);
				}
				break;
			case ItemRarity.epic:
				if (epicItems.Count > 0)
				{
					item = epicItems[0];
					epicItems.Remove(item);
				}
				else if (rareItems.Count > 0)
				{
					item = rareItems[0];
					rareItems.Remove(item);
				}
				else if (commonItems.Count > 0)
				{
					item = commonItems[0];
					commonItems.Remove(item);
				}
				break;
			default:
				item = null;
				break;
		}

		return item;
	}

	public void OnItemPicked(Item pickedItem)
	{
		for (int i = 0; i < currentlyVisible.Count; i++)
		{
			Item traverse = currentlyVisible[i];
			if (traverse != pickedItem)
			{
				traverse.gameObject.SetActive(false);
				if (traverse.itemRarity == ItemRarity.common)
				{
					commonItems.Add(traverse);
				}
				else if (traverse.itemRarity == ItemRarity.rare)
				{
					rareItems.Add(traverse);
				}
				else if (traverse.itemRarity == ItemRarity.epic)
				{
					epicItems.Add(traverse);
				}
			}
		}

		currentlyVisible.Clear();
		
		Inventory.instance.PutItemIntoInventory(pickedItem);
		Player.instance.PickItem(pickedItem);
	}

	void Init()
	{
		inited = true;
		for (int i = 0; i < itemPool.Count; i++)
		{
			if (itemPool[i].itemRarity == ItemRarity.common)
			{
				commonItems.Add(itemPool[i]);
			}
			else if (itemPool[i].itemRarity == ItemRarity.rare)
			{
				rareItems.Add(itemPool[i]);
			}
			else if (itemPool[i].itemRarity == ItemRarity.epic)
			{
				epicItems.Add(itemPool[i]);
			}
		}
		
	}

	public bool PoolHasItem()
	{
		return itemPool.Count > 0 || !inited;
	}
}



