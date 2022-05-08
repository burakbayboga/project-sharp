using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public static Inventory instance;
	public GameObject panel;

	void Awake()
	{
		instance = this;
	}

	public void PutItemIntoInventory(Item item)
	{
		item.description.SetActive(false);
		item.shortDescription.SetActive(true);
		item.transform.SetParent(panel.transform, false);
		item.inInventory = true;
	}

    public void OnInventoryClicked()
	{
		panel.SetActive(!panel.activeInHierarchy);
	}
}
