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
    public Text passTurnText;
    public Animator playerMoveAnimator;
    public Animator enemiesAnimator;
    public Animator answerAnimator;
    public Animator passTurnAnimator;

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
	public GameObject chargePanel;
    public GameObject clickEnemyTooltip;

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
	bool seenCharge;
	bool seenHighDamage;

	Skill registerPlayerAction_Skill;
	int registerPlayerAction_Damage;

	public override void Start()
	{
		Skill.InitSkills();
		
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
        enemiesAnimator.Play("text idle");
        answerAnimator.Play("text big small");


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
        if (clickEnemyTooltip != null)
        {
            clickEnemyTooltip.SetActive(true);
        }
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
		base.RegisterPlayerAction(registerPlayerAction_Skill, registerPlayerAction_Damage);
        answerText.color = Color.white;
        passTurnText.color = Color.green;
        answerAnimator.Play("text idle");
        passTurnAnimator.Play("text big small");
	}

	public void OnNextClicked_AnswerPanel_5()
	{
		answerPanel_5.SetActive(false);
		isTutorialPanelActive = false;
		base.RegisterPlayerAction(registerPlayerAction_Skill, registerPlayerAction_Damage);
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
		isTutorialPanelActive = false;
		TurnProgressButton.interactable = true;
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

	public void OnNextClicked_ChargePanel()
	{
		chargePanel.SetActive(false);
		isTutorialPanelActive = false;
		TurnProgressButton.interactable = false;
	}

	public override void RegisterPlayerAction(Skill reaction, int damage)
	{
        answerText.color = Color.white;
        passTurnText.color = Color.green;
        answerAnimator.Play("text idle");
        passTurnAnimator.Play("text big small");

		registerPlayerAction_Skill = reaction;
		registerPlayerAction_Damage = damage;
		if (seenAnswer4 && !seenAnswer5)
		{
			seenAnswer5 = true;
			answerPanel_5.SetActive(true);
			isTutorialPanelActive = true;
		}
		else if (!seenAnswer4)
		{
			seenAnswer4 = true;
			answerPanel_4.SetActive(true);
			isTutorialPanelActive = true;
		}
		else
		{
			base.RegisterPlayerAction(reaction, damage);
		}
	}

	protected override void EndTurn(bool forced = false)
	{
		if (!allowInjury)
		{
			if (EnemyManager.instance.GetUnansweredEnemyCount() > 0)
			{
				noAnswerPanel.SetActive(true);
				isTutorialPanelActive = true;
				return;
			}
		}

		Player.instance.currentHex.RevertAdjacentHighlights();
		unansweredEnemyText.gameObject.SetActive(false);
		TurnProgressButton.interactable = false;
		if (CurrentEnemy != null)
		{
			CurrentEnemy.currentHex.UnselectAsTarget();
		}
		UpdateTurnText();
        StartCoroutine(ProcessCombat());

		passTurnText.color = Color.white;
        passTurnAnimator.Play("text idle");
		SidestepSkillButton.gameObject.SetActive(false);
	}

	public override void OnEnemyClicked(Enemy enemy)
	{
        if (clickEnemyTooltip != null && clickEnemyTooltip.activeInHierarchy)
        {
            clickEnemyTooltip.SetActive(false);
        }

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
			Vector3 pos = CurrentEnemy.currentHex.transform.position - Hex.posOffset;
			pos.z = -6f;
			SkillsParent.transform.position = pos;

			SkillType enemyActionType = CurrentEnemy.CurrentAction != null ? CurrentEnemy.CurrentAction.Type : SkillType.None;

			bool isEnemyAdjacent = CurrentEnemy.currentHex.IsAdjacentToPlayer();
			bool isEnemyVulnerable = CurrentEnemy.IsVulnerable;
			bool isEnemyDefensive = CurrentEnemy.IsDefensive();
			bool isEnemyShootingArrow = CurrentEnemy.CurrentAction == Skill.ShootArrow;
			bool isEnemyIdle = CurrentEnemy.CurrentAction == null;
			bool isEnemyAnswered = CurrentEnemy.answeredThisTurn;
			bool isChargeAllowed = currentLevel == 2;

			((SkillCanvasTutorial)SkillCanvas.instance).HandleSkillsT(isEnemyAdjacent, isEnemyVulnerable, isEnemyDefensive, isEnemyShootingArrow, isEnemyIdle, enemyActionType, sidestepUsed, isEnemyAnswered, isChargeAllowed);

			if (isEnemyVulnerable && !isEnemyAnswered)
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

			if (seenCharge && currentLevel == 2 && !seenHighDamage)
			{
				idleEnemyPanel.SetActive(true);
				isTutorialPanelActive = true;
				seenHighDamage = true;
			}

			if (!seenCharge && currentLevel == 2)
			{
				chargePanel.SetActive(true);
				isTutorialPanelActive = true;
				seenCharge = true;
			}
		}
	}

	protected override void EnterPlayerMoveTurn()
	{
		base.EnterPlayerMoveTurn();
		playerMoveText.color = Color.green;
        playerMoveAnimator.Play("text big small");
	}

	public override void OnPlayerMove()
	{
		if (CurrentTurnState == TurnState.PlayerAnswer && !sidestepUsed)
		{
			injuryPanel.SetActive(true);
			isTutorialPanelActive = true;
			sidestepUsed = true;
			isSidestepActive = false;
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
				if (!Player.instance.sidestepUsed)
				{
					SidestepSkillButton.gameObject.SetActive(true);
					SkillCanvas.HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
				}
				else
				{
					SidestepSkillButton.gameObject.SetActive(false);
				}
				Player.instance.currentHex.RevertAdjacentHighlights();
				isSidestepActive = false;
			}
		}
        else
        {
            if (CurrentTurnState == TurnState.PlayerAnswer && !seenAnswer4)
            {
                if (clickEnemyTooltip != null)
                {
                    clickEnemyTooltip.SetActive(true);
                }
            }
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
        if (currentLevel == 0)
        {
            GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
            clickEnemyTooltip = Instantiate(clickEnemyTooltip, enemyObject.transform);
            clickEnemyTooltip.SetActive(false);
        }

	}

	protected override void MakeEnemiesMove()
	{
		playerMoveText.color = Color.white;
		enemiesText.color = Color.green;
        playerMoveAnimator.Play("text idle");

		if (!seenEnemies)
		{
			seenEnemies = true;
			enemiesPanel.SetActive(true);
			isTutorialPanelActive = true;
            enemiesAnimator.Play("text big small");
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
        enemiesAnimator.Play("text idle");
        answerAnimator.Play("text big small");

		if (pendingEnemyHeal && !seenEnemyHeal)
		{
			enemyHealPanel.SetActive(true);
			isTutorialPanelActive = true;
			seenEnemyHeal = true;
			allowInjury = false;
		}

		if (sidestepUnlocked)
		{
			SidestepSkillButton.gameObject.SetActive(!Player.instance.sidestepUsed);
			SkillCanvas.HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
		}

		if (pendingSidestep)
		{
			pendingSidestep = false;
			sidestepPanel.SetActive(true);
			isTutorialPanelActive = true;
			sidestepUnlocked = true;
			SidestepSkillButton.gameObject.SetActive(!Player.instance.sidestepUsed);
			SkillCanvas.HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
			pendingEnemyHeal = true;
		}
	}

	public override void UpdateTurnCountText(){  }

}
