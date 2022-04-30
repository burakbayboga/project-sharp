using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour
{
	
	public Hex[] adjacents;
	public bool isOccupied;
	public Color highlightColor;
	public Color baseColor;
	public Sprite baseSprite;
	public Sprite highlightSprite;

	Collider2D selfCollider;
	SpriteRenderer rend;

	void Awake()
	{
		selfCollider = GetComponent<Collider2D>();
		rend = GetComponent<SpriteRenderer>();
	}

	void Start()
	{
		SetAdjacents();
		SetInitialOccupier();
	}

	void SetInitialOccupier()
	{
		Collider2D collider = Physics2D.OverlapCircle(transform.position, 0.2f, 1 << 8);
		if (collider != null)
		{
			isOccupied = true;
			collider.transform.position = transform.position;
			if (collider.CompareTag("Player"))
			{
				collider.GetComponent<Player>().currentHex = this;
			}
		}
	}

	void SetAdjacents()
	{
		Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f, 1 << 9);
		adjacents = new Hex[colliders.Length - 1];
		
		int adjacentIndex = 0;
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] != selfCollider)
			{
				adjacents[adjacentIndex] = colliders[i].GetComponent<Hex>();
				adjacentIndex++;
			}
		}
	}

	public void HighlightValidAdjacents()
	{
		for (int i = 0; i < adjacents.Length; i++)
		{
			if (!adjacents[i].isOccupied)
			{
				adjacents[i].HighlightSelf();
			}
		}
	}

	public void HighlightSelf()
	{
		rend.color = highlightColor;	
		rend.sprite = highlightSprite;
	}
}
