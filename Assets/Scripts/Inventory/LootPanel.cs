using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LootPanel : MonoBehaviour
{
	public Item[] itemPrefabs;
	
	List<Item> itemPool = new List<Item>();

	bool inited = false;

	public void Fill()
	{
		if (!inited)
		{
			Init();
		}

		int count = Mathf.Min(2, itemPool.Count);
		itemPool = itemPool.OrderBy(r => Random.Range(0f, 1f) < 0.5f).ToList();
		for (int i = 0; i < count; i++)
		{
			itemPool[i].gameObject.SetActive(true);
		}
	}

	public void OnItemPicked(ItemType _itemType)
	{
		Item pickedItem = null;
		for (int i = 0; i < itemPool.Count; i++)
		{
			if (itemPool[i].itemType == _itemType)
			{
				pickedItem = itemPool[i];
				itemPool.RemoveAt(i);
				break;
			}
		}
		pickedItem.gameObject.SetActive(false);
		Player.instance.PickItem(pickedItem);
	}

	void Init()
	{
		inited = true;

		for (int i = 0; i < itemPrefabs.Length; i++)
		{
			itemPool.Add(Instantiate(itemPrefabs[i], transform).GetComponent<Item>());
			itemPool[i].gameObject.SetActive(false);
		}
	}

	public bool PoolHasItem()
	{
		return itemPool.Count > 0 || !inited;
	}
}


public enum ItemType
{
	shield = 0,
	parryingDagger = 1
}
