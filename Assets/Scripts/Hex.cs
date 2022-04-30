using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour
{
	
	public Hex[] adjacents;

	Collider2D selfCollider;

	void Awake()
	{
		selfCollider = GetComponent<Collider2D>();
	}

	void Start()
	{
		SetAdjacents();
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
}
