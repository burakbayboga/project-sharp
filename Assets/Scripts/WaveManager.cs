using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    public GameObject EnemyPrefab;

    void Awake()
    {
        instance = this;    
    }

    public void SendNewWave()
    {
        List<Enemy> newEnemies = new List<Enemy>();

		List<Hex> hexes = Player.instance.currentHex.GetAdjacentsWithRange(2);
		int enemyCount = GetEnemyCount();

        for (int i = 0; i < enemyCount; i++)
        {
			Hex spawnHex = hexes[Random.Range(0, hexes.Count)];
            Enemy enemy = Instantiate(EnemyPrefab, spawnHex.transform.position + Hex.posOffset, Quaternion.identity).GetComponent<Enemy>();
			enemy.currentHex = spawnHex;
			spawnHex.isOccupiedByEnemy = true;
            newEnemies.Add(enemy);
            enemy.TotalDurability = Random.Range(2, 5);
			hexes.Remove(spawnHex);
        }

		GameController.instance.RegisterNewEnemies(newEnemies);
    }

	int GetEnemyCount()
	{
		int wave = GameController.instance.currentWave;

		if (wave < 2)
		{
			return 3;
		}
		else if (wave < 5)
		{
			return 4;
		}
		else
		{
			return 5;
		}
	}
}
