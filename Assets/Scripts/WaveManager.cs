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

        int enemyCount = Random.Range(2, SpawnTransforms.Length);
        Enemy[] newEnemies = new Enemy[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
            Enemy enemy = Instantiate(EnemyPrefab, SpawnTransforms[i].position, Quaternion.identity).GetComponent<Enemy>();
            newEnemies[i] = enemy;
            enemy.TotalDurability = Random.Range(2, 5);
        }




        GameController.instance.RegisterEnemies(newEnemies);
    }

}
