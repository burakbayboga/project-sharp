using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
	static PriorityQueue openList;
	static List<ASN> closedList;
	static List<ASN> neighbors;

	static Hex startHex;
	static Hex endHex;

	public static Hex GetHexFirstInPath(Hex _endHex, Hex _startHex)
	{
		openList = new PriorityQueue();
		closedList = new List<ASN>();
		neighbors = new List<ASN>();
		startHex = _startHex;
		endHex = _endHex;

		ASN endNode = new ASN();
		endNode.hex = endHex;
		endNode.cost = 0f;
		endNode.parent = endNode;
		openList.Enqueue(endNode);

		ASN current;
		while (!openList.IsEmpty())
		{
			current = openList.Dequeue();
			closedList.Add(current);

			if (current.hex == startHex)
			{
				return GetHexFromPath(current);
			}

			UpdateNeighbors(current.hex);
			for (int i = 0; i < neighbors.Count; i++)
			{
				ASN nodeN = neighbors[i];
				if (!ListContains(closedList, nodeN))
				{
					if (!openList.Contains(nodeN))
					{
						nodeN.cost = float.MaxValue;
						nodeN.parent = null;
					}

					ProcessNode(current, nodeN);
				}
			}
		}

		return null;
	}

	static void ProcessNode(ASN current, ASN nodeN)
	{
		float oldCost = nodeN.cost;
		CalculateCost(current, nodeN);
		if (nodeN.cost < oldCost)
		{
			if (openList.Contains(nodeN))
			{
				openList.Remove(nodeN);
			}
			openList.Enqueue(nodeN);
		}
	}

	static void CalculateCost(ASN current, ASN nodeN)
	{
		float localCost = Vector3.Distance(current.hex.transform.position, nodeN.hex.transform.position) + Vector3.Distance(nodeN.hex.transform.position, startHex.transform.position);
		
		if ((current.cost + localCost) < nodeN.cost)
		{
			nodeN.parent = current;
			nodeN.cost = current.cost + localCost;
		}
	}

	static void UpdateNeighbors(Hex hex)
	{
		neighbors.Clear();
		for (int i = 0; i < hex.adjacents.Length; i++)
		{
			ASN node = new ASN();
			node.hex = hex.adjacents[i];
			neighbors.Add(node);
		}
	}

	static Hex GetHexFromPath(ASN current)
	{
		List<Hex> path = new List<Hex>();
		while (current.hex != endHex)
		{
			path.Add(current.hex);
			current = current.parent;
		}
		return path[path.Count - 1];
	}

	static bool ListContains(List<ASN> list, ASN node)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].hex == node.hex)
			{
				return true;
			}
		}
		return false;
	}
}
