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

	public bool isHighlighted;

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
			else if (collider.CompareTag("Enemy"))
			{
				collider.GetComponent<Enemy>().currentHex = this;
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

	public void HandleInput()
	{
		if (isHighlighted)
		{
			GameController.instance.OnPlayerMove();
			Player.instance.currentHex.RevertAdjacentHighlights();
			Player.instance.transform.position = transform.position;
			Player.instance.currentHex = this;
		}
	}

	public void RevertAdjacentHighlights()
	{
		for (int i = 0; i < adjacents.Length; i++)
		{
			adjacents[i].RevertHighlight();
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

	public void RevertHighlight()
	{
		rend.color = baseColor;
		rend.sprite = baseSprite;
		isHighlighted = false;
	}

	public void HighlightSelf()
	{
		rend.color = highlightColor;	
		rend.sprite = highlightSprite;
		isHighlighted = true;
	}
}
