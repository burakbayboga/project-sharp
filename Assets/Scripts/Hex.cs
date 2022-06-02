using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Hex : MonoBehaviour
{
	public Hex[] adjacents;
	public Color highlightColor;
	public Color selectedColor;
	public Color baseColor;
	public Sprite baseSprite;
	public Sprite highlightSprite;
	public bool isOccupiedByPlayer;
	public GameObject affectedBySkillEffect;
	public Enemy enemy;

	public static Vector3 posOffset = new Vector3(0.04f, 0.09f, 0f);
	public static float hexOffsetX = 0.8660254f;	//いいだよ

	public bool isOccupied
	{
		get
		{
			return isOccupiedByPlayer || enemy != null;
		}
	}

	Collider2D selfCollider;
	SpriteRenderer rend;

	public bool isHighlighted;

	void Awake()
	{
		selfCollider = GetComponent<Collider2D>();
		rend = GetComponent<SpriteRenderer>();
		SetAdjacents();
		SetInitialOccupier();
	}

	void SetInitialOccupier()
	{
		Collider2D collider = Physics2D.OverlapCircle(transform.position, 0.3f, 1 << 8);
		if (collider != null)
		{
			collider.transform.position = transform.position + posOffset;
			if (collider.CompareTag("Enemy"))
			{
				enemy = collider.GetComponent<Enemy>();
				enemy.currentHex = this;
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

	public int GetAdjacentEnemyCount()
	{
		int enemyCount = 0;
		for (int i = 0; i < adjacents.Length; i++)
		{
			if (adjacents[i].enemy != null)
			{
				enemyCount++;
			}
		}

		return enemyCount;
	}

	public void HandleInput()
	{
		if (isHighlighted)
		{
			Player.instance.MovePlayer(this, GameController.instance.CurrentTurnState == TurnState.PlayerAnswer, false);
			GameController.instance.OnPlayerMove();
		}
		else
		{
			GameController.instance.OnEmptyClick();
		}
	}

	public bool HasGapBetweenHex(Hex hex)
	{
		Ray ray = new Ray(transform.position, hex.transform.position - transform.position);
		float distance = Vector3.Distance(transform.position, hex.transform.position);
		return Physics2D.RaycastAll(ray.origin, ray.direction, distance, 1 << 14).Length > 0;
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

	public void SetAffectedBySkill(bool active)
	{
		affectedBySkillEffect.SetActive(active);
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

	public void SelectAsTarget()
	{
		rend.color = selectedColor;
		rend.sprite = highlightSprite;
	}

	public void UnselectAsTarget()
	{
		rend.color = baseColor;
		rend.sprite = baseSprite;
	}

	public void HighlightSelf()
	{
		rend.color = highlightColor;	
		rend.sprite = highlightSprite;
		isHighlighted = true;
	}
}
