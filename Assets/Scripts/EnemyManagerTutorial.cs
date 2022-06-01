using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManagerTutorial : EnemyManager
{
 	public override int KillMarkedEnemies(out bool willSendNewWave, ref bool pendingLootTurn, ref int killsUntilNextItem, ref bool pendingNewLevel, ref int turnCount, ref int currentWave, ref bool isLastLevel, ref bool loadLevelAtTurnEnd)
	{
		willSendNewWave = false;
		if (EnemiesMarkedForDeath.Count > 0 && isLastLevel)
		{
			Instantiate(BloodEffectPrefab, Enemies[0].transform.position, Quaternion.identity);
			GameObject splatter = Instantiate(splatterPrefabs[Random.Range(0, splatterPrefabs.Length)], Enemies[0].currentHex.transform.position, Quaternion.identity);
			splatter.transform.SetParent(GameController.instance.loadedLevel.transform);
			Destroy(Enemies[0].gameObject);
			((InteractiveTutorial)GameController.instance).idleEnemyPanel.SetActive(true);
			((InteractiveTutorial)GameController.instance).isTutorialPanelActive = true;
			return 1;
		}
		else
		{
			return base.KillMarkedEnemies(out willSendNewWave, ref pendingLootTurn, ref killsUntilNextItem, ref pendingNewLevel, ref turnCount, ref currentWave, ref isLastLevel, ref loadLevelAtTurnEnd);
		}
	}   
}
