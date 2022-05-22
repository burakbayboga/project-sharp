using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public static Inventory instance;
	public GameObject panel;
	public Transform contentParent;

	void Awake()
	{
		instance = this;
	}

	public void PutItemIntoInventory(Item item)
	{
		item.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
		item.transform.SetParent(contentParent, false);
		item.inInventory = true;
	}

    public void OnInventoryClicked()
	{
		panel.SetActive(!panel.activeInHierarchy);
	}

	public void SetInventoryActive(bool active)
	{
		panel.SetActive(active);
	}
}
