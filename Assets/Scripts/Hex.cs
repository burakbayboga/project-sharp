using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Hex : MonoBehaviour
{
	
	public Hex[] adjacents;
	public Color highlightColor;
	public Color baseColor;
	public Sprite baseSprite;
	public Sprite highlightSprite;
	public bool isOccupiedByEnemy;
	public bool isOccupiedByPlayer;

	public static Vector3 posOffset = new Vector3(0.04f, 0.09f, 0f);

	public bool isOccupied
	{
		get
		{
			return isOccupiedByEnemy || isOccupiedByPlayer;
		}
	}

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
			collider.transform.position = transform.position + posOffset;
			if (collider.CompareTag("Player"))
			{
				collider.GetComponent<Player>().currentHex = this;
				isOccupiedByPlayer = true;
			}
			else if (collider.CompareTag("Enemy"))
			{
				collider.GetComponent<Enemy>().currentHex = this;
				isOccupiedByEnemy = true;
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

	public List<Hex> GetAdjacentsWithRange(int range)
	{
		Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, 1 << 9);
		List<Hex> hexes = colliders.Select(c => c.GetComponent<Hex>()).Where(h => !h.isOccupied).ToList();

		return hexes;
	}

	public void HandleInput()
	{
		if (isHighlighted)
		{
			Player.instance.currentHex.RevertAdjacentHighlights();
			Player.instance.currentHex.isOccupiedByPlayer = false;
			Player.instance.transform.position = transform.position + posOffset;
			Player.instance.currentHex = this;
			isOccupiedByPlayer = true;
			GameController.instance.OnPlayerMove();
		}
	}

	public bool IsAdjacentToPlayer()
	{
		for (int i = 0; i < adjacents.Length; i++)
		{
			if (adjacents[i].isOccupiedByPlayer)
			{
				return true;
			}
		}

		return false;
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
