﻿using System.Collections;
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
		List<Hex> hexes = Player.instance.currentHex.GetAdjacentsWithRange(2);

        int enemyCount = Random.Range(2, Mathf.Min(5, hexes.Count));
        List<Enemy> newEnemies = new List<Enemy>();

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
}
