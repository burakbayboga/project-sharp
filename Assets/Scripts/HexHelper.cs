using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexHelper
{
    public static Hex GetNewHexForChargingPlayer(Hex targetHex)
	{
		Hex traverse = Player.instance.currentHex;
		float realStartTime = Time.realtimeSinceStartup;
		while (true)
		{
			if (Time.realtimeSinceStartup - realStartTime > 10f)
			{
				Debug.LogError("new hex for charging player safety net");
				break;
			}

			Hex next;
			Ray ray = new Ray(traverse.transform.position, targetHex.transform.position - traverse.transform.position);
			RaycastHit2D[] hits = Physics2D.CircleCastAll(ray.origin, 0.25f, ray.direction, 0.5f, 1 << 9);
			Hex nextCandidate = GetSuitableHexForCreatureFromHits(hits, traverse);
			if (nextCandidate != null)
			{
				next = nextCandidate;
				if (next == targetHex)
				{
					break;
				}
				else
				{
					traverse = next;
				}
			}
			else
			{
				break;
			}
		}

		return traverse;
	}

	static Hex GetSuitableHexForCreatureFromHits(RaycastHit2D[] hits, Hex baseHex)
	{
		for (int i = 0; i < hits.Length; i++)
		{
			Hex candidate = hits[i].collider.GetComponent<Hex>();
			if (candidate != baseHex && candidate.enemy == null)
			{
				return candidate;
			}
		}

		return null;
	}

	public static Hex GetNewHexForShovedEnemy(Enemy enemy)
	{
		Hex traverse = enemy.currentHex;
		Hex playerHex = Player.instance.currentHex;
		Vector3 direction = traverse.transform.position - playerHex.transform.position;

		for (int i = 0; i < 2; i++)
		{
			Ray ray = new Ray(traverse.transform.position, direction);
			RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 1f, 1 << 9);
			Hex next = GetSuitableHexForCreatureFromHits(hits, traverse);
			if (next != null)
			{
				traverse = next;
			}
			else
			{
				break;
			}
		}

		return traverse;
	}

	public static Hex GetNewHexForHookedEnemy(Enemy enemy)
	{
		Hex traverse = enemy.currentHex;
		Hex playerHex = Player.instance.currentHex;
		float realStartTime = Time.realtimeSinceStartup;
		while (true)
		{
			if (Time.realtimeSinceStartup - realStartTime > 10f)
			{
				Debug.LogError("new hex for hooked enemy safety net");
				// safety net
				break;
			}
			Hex next;
			Ray ray = new Ray(traverse.transform.position, playerHex.transform.position - traverse.transform.position);
			RaycastHit2D[] hits = Physics2D.CircleCastAll(ray.origin, 0.25f, ray.direction, 0.5f, 1 << 9);
			Hex nextCandidate = GetSuitableHexForCreatureFromHits(hits, traverse);
			if (nextCandidate != null)
			{
				next = nextCandidate;
				if (next.isOccupiedByPlayer)
				{
					break;
				}
				else
				{
					traverse = next;
				}
			}
			else
			{
				break;
			}
		}

		return traverse;
	}
}
