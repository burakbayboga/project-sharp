using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// A Star Node
public class ASN
{
	public Hex hex;
	public float cost;
	public ASN parent;
}

public class PriorityQueue
{
	List<ASN> queue;

	public PriorityQueue()
	{
		queue = new List<ASN>();
	}

	public void Enqueue(ASN node)
	{
		queue.Add(node);
		queue = queue.OrderBy(n => n.cost).ToList();
	}

	public ASN Dequeue()
	{
		ASN node = queue[0];
		queue.RemoveAt(0);
		return node;
	}

	public bool Contains(ASN node)
	{
		for (int i = 0; i < queue.Count; i++)
		{
			if (queue[i].hex == node.hex)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEmpty()
	{
		return queue.Count == 0;
	}

	public void Remove(ASN node)
	{
		queue.Remove(node);
	}
}
