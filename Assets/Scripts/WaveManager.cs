using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    public GameObject[] EnemyPrefabs;

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
            Enemy enemy = Instantiate(EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)], spawnHex.transform.position + Hex.posOffset, Quaternion.identity).GetComponent<Enemy>();
			enemy.Init(spawnHex);
            newEnemies.Add(enemy);
			hexes.Remove(spawnHex);
        }

		EnemyManager.instance.RegisterNewEnemies(newEnemies);
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
