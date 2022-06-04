using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

	public GameObject BloodEffectPrefab;
	public GameObject[] splatterPrefabs;
	public LootPanel LootPanel;

	public List<Enemy> Enemies = new List<Enemy>();
    protected List<Enemy> EnemiesMarkedForDeath = new List<Enemy>();

	void Awake()
	{
		instance = this;
	}

	public virtual int KillMarkedEnemies(out bool willSendNewWave, ref bool pendingLootTurn, ref int killsUntilNextItem, ref bool pendingNewLevel, ref int turnCount, ref int currentWave, ref bool isLastLevel, ref bool loadLevelAtTurnEnd)
    {
		willSendNewWave = false;
		int killedEnemyCount = EnemiesMarkedForDeath.Count;
        while (EnemiesMarkedForDeath.Count > 0)
        {
            Enemy temp = EnemiesMarkedForDeath[0];
            Enemies.Remove(temp);
            EnemiesMarkedForDeath.Remove(temp);
            Instantiate(BloodEffectPrefab, temp.transform.position, Quaternion.identity);
			GameObject splatter = Instantiate(splatterPrefabs[Random.Range(0, splatterPrefabs.Length)], temp.currentHex.transform.position, Quaternion.identity);
			splatter.transform.SetParent(GameController.instance.loadedLevel.transform);
			temp.currentHex.enemy = null;
            Destroy(temp.gameObject);

			killsUntilNextItem--;
			if (killsUntilNextItem == 0)
			{
				pendingLootTurn = true;
				killsUntilNextItem = GameController.instance.killsRequiredForNewItem;
				Player.instance.ResetInjuries();
			}
        }

		turnCount--;
        if (Enemies.Count == 0 || turnCount == 0)
        {
			if (currentWave % 2 == 0)
			{
				LootPanel.IncreaseItemQuality();
			}
			turnCount = GameController.instance.turnLimitForNewWave;
			GameController.instance.UpdateTurnCountText();
			if (currentWave == GameController.instance.waveLimitForLevel && isLastLevel)
			{
				print("now entering endless waves");
			}
			if (currentWave == GameController.instance.waveLimitForLevel && !isLastLevel)
			{
				if (Enemies.Count == 0)
				{
					loadLevelAtTurnEnd = true;
				}
				else
				{
					pendingNewLevel = true;
				}
			}

			if (!pendingNewLevel && !loadLevelAtTurnEnd)
			{
				willSendNewWave = true;
				WaveManager.instance.SendNewWave();
				currentWave++;
				if (currentWave == GameController.instance.waveLimitForLevel)
				{
					turnCount = -1;
				}
			}
        }

		return killedEnemyCount;
    }

	public void ResetEnemies()
	{
		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].ResetEnemy();
		}
	}

	public void MoveEnemies()
	{
		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].MoveTurn();
		}
	}

	public void PickActionForEnemies()
	{
		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].PickAction();
		}
	}

	public void CheckEnemyActionsValidity()
	{
		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].CheckActionValidity();
		}
	}

	public int GetUnansweredEnemyCount()
	{
		int count = 0;
		for (int i = 0; i < Enemies.Count; i++)
		{
			if (Enemies[i].CurrentAction != null && Enemies[i].CurrentPlayerReaction == null)
			{
				count++;
			}
		}

		return count;
	}

	public void RegisterFirstEnemies()
	{
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
			Enemies.Add(enemies[i].GetComponent<Enemy>());
			Enemies[i].Init(Enemies[i].currentHex);	// wow
        }
	}

	public void RegisterNewEnemies(List<Enemy> newEnemies)
	{
		Enemies.AddRange(newEnemies);
		GameController.instance.UpdateUnansweredEnemyText();
	}

    public void MarkEnemyForDeath(Enemy enemy)
    {
        EnemiesMarkedForDeath.Add(enemy);
    }
}
