using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{

    public static WaveManager instance;

    public GameObject EnemyPrefab;
    public Transform[] SpawnTransforms;

    void Awake()
    {
        instance = this;    
    }

    public void SendNewWave()
    {
		List<Hex> hexes = Player.instance.currentHex.GetAdjacentsWithRange(2);

        int enemyCount = Random.Range(2, Mathf.Min(5, hexes.Count));
        Enemy[] newEnemies = new Enemy[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
			Hex spawnHex = hexes[Random.Range(0, hexes.Count)];
            Enemy enemy = Instantiate(EnemyPrefab, spawnHex.transform.position, Quaternion.identity).GetComponent<Enemy>();
			enemy.currentHex = spawnHex;
			spawnHex.isOccupiedByEnemy = true;
            newEnemies[i] = enemy;
            enemy.TotalDurability = Random.Range(2, 5);
			hexes.Remove(spawnHex);
        }

        GameController.instance.RegisterEnemies(newEnemies);
    }

}
