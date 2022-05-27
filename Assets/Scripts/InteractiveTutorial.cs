using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InteractiveTutorial : GameController
{

	public Text playerMoveText;
	public Text enemiesText;
	public Text answerText;
	public Text clashText;

	public GameObject movePanel;
	public GameObject enemiesPanel;
	public GameObject answerPanel_1;
	public GameObject answerPanel_2;
	public GameObject answerPanel_3;
	public GameObject answerPanel_4;
	public GameObject answerPanel_5;
	public GameObject killPanel;
	public GameObject noAnswerPanel;
	public GameObject sidestepPanel;
	public GameObject injuryPanel;
	public GameObject enemyHealPanel;
	public GameObject idleEnemyPanel;
	public GameObject readyToPlayPanel;
	public GameObject losPanel;


	bool seenEnemies;
	bool seenAnswer3;
	bool seenAnswer4;
	bool seenAnswer5;
	bool seenKillPanel;
	bool allowInjury;
	bool pendingSidestep;
	bool sidestepUnlocked;
	bool sidestepUsed;
	bool seenEnemyHeal;
	bool pendingEnemyHeal;

	public override void Start()
	{
		waveLimitForLevel = 0;
		Skill.InitSkills();
		lootPanel.Init();
		
		StartCoroutine(LoadNextLevel(true));
		playerMoveText.color = Color.green;
		movePanel.SetActive(true);
		isTutorialPanelActive = true;
	}

	public void OnNextClicked_MovePanel()
	{
		movePanel.SetActive(false);
		isTutorialPanelActive = false;
		EnterPlayerMoveTurn();
	}

	public void OnNextClicked_EnemiesPanel()
	{
		base.MakeEnemiesMove();
		enemiesPanel.SetActive(false);
		enemiesText.color = Color.white;
		answerText.color = Color.green;

		answerPanel_1.SetActive(true);
	}

	public void OnNextClicked_AnswerPanel_1()
	{
		answerPanel_1.SetActive(false);
		answerPanel_2.SetActive(true);
	}

	public void OnNextClicked_AnswerPanel_2()
	{
		answerPanel_2.SetActive(false);
		isTutorialPanelActive = false;
		EnterEnemyActionTurn();
	}

	public void OnNextClicked_AnswerPanel_3()
	{
		answerPanel_3.SetActive(false);
		isTutorialPanelActive = false;
	}

	public void OnNextClicked_AnswerPanel_4()
	{
		answerPanel_4.SetActive(false);
		isTutorialPanelActive = false;
	}

	public void OnNextClicked_AnswerPanel_5()
	{
		answerPanel_5.SetActive(false);
		isTutorialPanelActive = false;
	}

	public void OnNextClicked_KillPanel()
	{
		killPanel.SetActive(false);
		isTutorialPanelActive = false;
	}

	public void OnNextClicked_NoAnswerPanel()
	{
		noAnswerPanel.SetActive(false);
		isTutorialPanelActive = false;
	}

	public void OnNextClicked_SidestepPanel()
	{
		sidestepPanel.SetActive(false);
		isTutorialPanelActive = false;
	}

	public void OnNextClicked_InjuryPanel()
	{
		injuryPanel.SetActive(false);
		isTutorialPanelActive = false;
	}

	public void OnNextClicked_EnemyHealPanel()
	{
		enemyHealPanel.SetActive(false);
		isTutorialPanelActive = false;
	}

	public void OnNextClicked_IdleEnemyPanel()
	{
		idleEnemyPanel.SetActive(false);
		readyToPlayPanel.SetActive(true);
	}

	public void OnNextClicked_ReadyToPlayPanel()
	{
		readyToPlayPanel.SetActive(false);
		isTutorialPanelActive = false;
		SceneManager.LoadScene("game");
		PlayerPrefs.SetInt("seenTutorial", 1);
		PlayerPrefs.Save();
	}

	public void OnNextClicked_LosPanel()
	{
		losPanel.SetActive(false);
		isTutorialPanelActive = false;
	}

	public override void RegisterPlayerAction(Skill reaction, int damage, Resource skillCost)
	{
		base.RegisterPlayerAction(reaction, damage, skillCost);
		if (seenAnswer4 && !seenAnswer5)
		{
			seenAnswer5 = true;
			answerPanel_5.SetActive(true);
			isTutorialPanelActive = true;
		}
		if (!seenAnswer4)
		{
			seenAnswer4 = true;
			answerPanel_4.SetActive(true);
			isTutorialPanelActive = true;
		}
	}

	protected override void EndTurn()
	{
		if (!allowInjury)
		{
			for (int i = 0; i < Enemies.Count; i++)
			{
				if (Enemies[i].CurrentAction != null && Enemies[i].CurrentPlayerReaction == null)
				{
					noAnswerPanel.SetActive(true);
					isTutorialPanelActive = true;
					return;
				}
			}
		}
		Player.instance.currentHex.RevertAdjacentHighlights();
		unansweredEnemyText.gameObject.SetActive(false);
		TurnProgressButton.interactable = false;
		if (CurrentEnemy != null)
		{
			CurrentEnemy.currentHex.UnselectAsTarget();
		}
		CurrentTurnState = TurnState.Clash;
		UpdateTurnText();
        StartCoroutine(ProcessCombat());

		answerText.color = Color.white;
		clashText.color = Color.green;
		SidestepSkillButton.gameObject.SetActive(false);

	}

	public override void OnEnemyClicked(Enemy enemy)
	{
		if (CurrentTurnState == TurnState.PlayerAnswer)
		{
			SidestepSkillButton.gameObject.SetActive(false);
			if (CurrentEnemy != null)
			{
				CurrentEnemy.currentHex.UnselectAsTarget();
			}
			CurrentEnemy = enemy;
			CurrentEnemy.currentHex.SelectAsTarget();
			SkillsParent.SetActive(true);

			SkillType enemyActionType = CurrentEnemy.CurrentAction != null ? CurrentEnemy.CurrentAction.Type : SkillType.None;
			HandleButtonIconsForSkill(Skill.SwiftAttack, enemyActionType, SwiftAttackSkillButton);
			HandleButtonIconsForSkill(Skill.Block, enemyActionType, BlockSkillButton);
			HandleButtonIconsForSkill(Skill.KillingBlow, enemyActionType, KillingBlowSkillButton);

			bool isEnemyAdjacent = CurrentEnemy.currentHex.IsAdjacentToPlayer();
			bool isEnemyVulnerable = CurrentEnemy.IsVulnerable;
			bool isEnemyDefensive = CurrentEnemy.IsDefensive();
			bool isEnemyShootingArrow = CurrentEnemy.CurrentAction == Skill.ShootArrow;
			bool isEnemyIdle = CurrentEnemy.CurrentAction == null;

			SwiftAttackSkillButton.gameObject.SetActive(isEnemyAdjacent);
			BlockSkillButton.gameObject.SetActive(isEnemyAdjacent && !isEnemyDefensive && !isEnemyShootingArrow && !isEnemyIdle);
			KillingBlowSkillButton.gameObject.SetActive(isEnemyVulnerable && isEnemyAdjacent);

			if (KillingBlowSkillButton.gameObject.activeSelf)
			{
				KillingBlowSkillButton.SetIndicatorAnimation(isEnemyVulnerable);
			}

			if (isEnemyVulnerable)
			{
				if (!seenKillPanel)
				{
					killPanel.SetActive(true);
					isTutorialPanelActive = true;
					seenKillPanel = true;
				}
			}

			if (!seenAnswer3)
			{
				answerPanel_3.SetActive(true);
				isTutorialPanelActive = true;
				seenAnswer3 = true;
			}
		}
	}

	protected override void EnterPlayerMoveTurn()
	{
		base.EnterPlayerMoveTurn();
		playerMoveText.color = Color.green;
		clashText.color = Color.white;
	}

	public override void OnPlayerMove()
	{
		if (CurrentTurnState == TurnState.PlayerAnswer && !sidestepUsed)
		{
			injuryPanel.SetActive(true);
			isTutorialPanelActive = true;
			sidestepUsed = true;
			Player.instance.CurrentResource = new Resource
			{
				Focus = 0,
				Strength = 0,
				Stability = 0
			};
			Player.instance.HandleResourceIcons();
			allowInjury = true;
			SidestepSkillButton.gameObject.SetActive(false);
		}
		else
		{
			base.OnPlayerMove();
		}
	}

	public override void OnEmptyClick()
	{
        SkillsParent.SetActive(false);
        if (CurrentEnemy != null)
        {
            CurrentEnemy.currentHex.UnselectAsTarget();
			CurrentEnemy.currentHex.SetAffectedBySkill(false);
        }
        CurrentEnemy = null;

		if (sidestepUnlocked)
		{
			if (CurrentTurnState == TurnState.PlayerAnswer)
			{
				SidestepSkillButton.gameObject.SetActive(!Player.instance.sidestepUsed);
				HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
				Player.instance.currentHex.RevertAdjacentHighlights();
				isSidestepActive = false;
			}
		}
	}

	protected override int KillMarkedEnemies()
	{
		if (EnemiesMarkedForDeath.Count > 0 && isLastLevel)
		{
			Instantiate(BloodEffectPrefab, Enemies[0].transform.position, Quaternion.identity);
			GameObject splatter = Instantiate(splatterPrefabs[Random.Range(0, splatterPrefabs.Length)], Enemies[0].currentHex.transform.position, Quaternion.identity);
			splatter.transform.SetParent(loadedLevel.transform);
			Destroy(Enemies[0].gameObject);
			idleEnemyPanel.SetActive(true);
			isTutorialPanelActive = true;
			return 1;
		}
		else
		{
			return base.KillMarkedEnemies();
		}
	}

	protected override IEnumerator LoadNextLevel(bool isFirstLevel = false)
	{
		yield return StartCoroutine(base.LoadNextLevel(isFirstLevel));
		if (currentLevel == 1)
		{
			pendingSidestep = true;
			losPanel.SetActive(true);
			isTutorialPanelActive = true;
		}
	}

	protected override void MakeEnemiesMove()
	{
		playerMoveText.color = Color.white;
		enemiesText.color = Color.green;

		if (!seenEnemies)
		{
			seenEnemies = true;
			enemiesPanel.SetActive(true);
			isTutorialPanelActive = true;
		}
		else
		{
			base.MakeEnemiesMove();
		}
	}

	protected override void EnterPlayerAnswerTurn()
	{
		CurrentTurnState = TurnState.PlayerAnswer;
		UpdateTurnText();
		UpdateUnansweredEnemyText();
		unansweredEnemyText.gameObject.SetActive(true);
		enemiesText.color = Color.white;
		answerText.color = Color.green;

		if (pendingEnemyHeal && !seenEnemyHeal)
		{
			enemyHealPanel.SetActive(true);
			isTutorialPanelActive = true;
			seenEnemyHeal = true;
		}

		if (sidestepUnlocked)
		{
			SidestepSkillButton.gameObject.SetActive(!Player.instance.sidestepUsed);
			HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
		}

		if (pendingSidestep)
		{
			pendingSidestep = false;
			sidestepPanel.SetActive(true);
			isTutorialPanelActive = true;
			sidestepUnlocked = true;
			SidestepSkillButton.gameObject.SetActive(!Player.instance.sidestepUsed);
			HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
			pendingEnemyHeal = true;
		}
	}

	protected override void UpdateTurnCountText(){  }

}
