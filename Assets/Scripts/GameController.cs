using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController instance;

	public GameObject[] levels;

    public TextMeshProUGUI TurnStateText;
    public GameObject SkillsParent;
	public GameObject unansweredEnemiesPanel;
	public LootPanel LootPanel;

	public GameObject actionCamera;
	public GameObject actionCanvas;
	public GameObject tutorialCanvas;

	public bool isHowToPlayActive;
	public bool isTutorialPanelActive;
	public bool isIngameInputActive;
    
	public SkillButton SidestepSkillButton;
	public SkillButton JumpSkillButton;

	// TODO: skill unlock system
	public bool isSkewerUnlocked;
	public bool isBlockArrowUnlocked;
	public bool isWhirlwindUnlocked;
	public bool isHookUnlocked;
	public bool isWrestleUnlocked;
	public bool isHeartshotUnlocked;
	public bool isLightningReflexesUnlocked;
	public bool isChargeUnlocked;
	public bool isJumpUnlocked;

	bool showUnansweredEnemiesPanel;

    public Button TurnProgressButton;
	public Text unansweredEnemyText;
	public Text turnCountText;
	public GameObject turnCountTextHeader;
	int turnCount;
	public int turnLimitForNewWave;
	public int waveLimitForLevel;

	public int killsRequiredForNewItem;
	int killsUntilNextItem;

    public GameObject DeathPanel;

    protected Enemy CurrentEnemy;

	Enemy enemyBeingCharged;

    public TurnState CurrentTurnState;

    public bool IsGameOver;

	public bool isSidestepActive = false;
	public bool isJumpActive = false;

	protected int currentLevel = -1;

	bool pendingLootTurn;
	public int currentWave = 0;
	bool pendingNewLevel;
	bool loadLevelAtTurnEnd;
	int characterBuild;
	bool buildingCharacter;
	protected bool isLastLevel;

	public GameObject loadedLevel;

    void Awake()
    {
        instance = this;
        IsGameOver = false;
		isIngameInputActive = true;
    }

    public virtual void Start()
    {
        CurrentTurnState = TurnState.Loot;
		UpdateTurnText();
		turnCount = turnLimitForNewWave;
		UpdateTurnCountText();

		StartCoroutine(LoadNextLevel(true));

        Skill.InitSkills();
		LootPanel.Init();
		killsUntilNextItem = killsRequiredForNewItem;

		StartCharacterBuild();

		showUnansweredEnemiesPanel = PlayerPrefs.GetInt("showUnansweredPanel", 1) == 1;
    }

	void StartCharacterBuild()
	{
		TurnProgressButton.interactable = false;
		buildingCharacter = true;
		characterBuild = 0;
		LootPanel.Fill(2);
		LootPanel.gameObject.SetActive(true);
		Inventory.instance.SetInventoryActive(true);
	}

	public virtual void UpdateTurnCountText()
	{
		if (turnCount < 0)
		{
			turnCountText.text = "Last Wave";
			turnCountTextHeader.SetActive(false);
		}
		else
		{
			turnCountText.text = turnCount.ToString();
			turnCountTextHeader.SetActive(true);
		}
	}

	public void UpdateTurnText()
	{
		TurnStateText.text = GetTurnText();
	}

	string GetTurnText()
	{
		switch (CurrentTurnState)
		{
			case TurnState.PlayerMovement:
				return "Player Movement";
			case TurnState.EnemyMovement:
				return "Enemy Movement";
			case TurnState.EnemyAction:
				return "Enemy Action";
			case TurnState.PlayerAnswer:
				return "Player Answer";
			case TurnState.Loot:
				return "Loot";
			default:
				return "TURN STATE ERROR";
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (TurnProgressButton.interactable)
			{
				OnEmptyClick();
				ProgressTurn();
			}
		}
	}

	public void UpdateUnansweredEnemyText()
	{
		unansweredEnemyText.text = EnemyManager.instance.GetUnansweredEnemyCount().ToString() + " Enemies\nNot Answered";
	}

	protected virtual void MakeEnemiesMove()
	{
		CurrentTurnState = TurnState.EnemyMovement;
		UpdateTurnText();
		EnemyManager.instance.MoveEnemies();
		ProgressTurn();
	}

	protected virtual void EnterPlayerMoveTurn()
	{
		CurrentTurnState = TurnState.PlayerMovement;
		UpdateTurnText();
		Player.instance.currentHex.HighlightValidAdjacents();
	}

	public virtual void OnPlayerMove()
	{
		if (CurrentTurnState == TurnState.PlayerMovement)
		{
			ProgressTurn();
		}
		else if (CurrentTurnState == TurnState.PlayerAnswer)
		{
			if (isSidestepActive)
			{
				Player.instance.OnPlayerSidestep();
				isSidestepActive = false;
				SidestepSkillButton.gameObject.SetActive(false);
				EnemyManager.instance.CheckEnemyActionsValidity();		
				UpdateUnansweredEnemyText();
			}
			else if (isJumpActive)
			{
				Player.instance.OnPlayerJump();
				isJumpActive = false;
				JumpSkillButton.gameObject.SetActive(false);
				EnemyManager.instance.CheckEnemyActionsValidity();
				UpdateUnansweredEnemyText();
			}
		}
	}

	public void OnCharge()
	{
		Hex hex = HexHelper.GetNewHexForChargingPlayer(CurrentEnemy.currentHex);
		Player.instance.MovePlayer(hex);
		EnemyManager.instance.CheckEnemyActionsValidity();
		enemyBeingCharged.ForceCancelAction();
		UpdateUnansweredEnemyText();
		OnEmptyClick();
	}

	public void OnWrestle()
	{
		CurrentEnemy.currentHex.SetAffectedBySkill(false);
		Hex playerHex = Player.instance.currentHex;
		Player.instance.MovePlayer(CurrentEnemy.currentHex, false, true);
		CurrentEnemy.MoveToHex(playerHex, true);

		EnemyManager.instance.CheckEnemyActionsValidity();
		//CurrentEnemy.ForceCancelAction();
		UpdateUnansweredEnemyText();
		OnEmptyClick();
	}

    void ProgressTurn()
    {
		if (isTutorialPanelActive)
		{
			return;
		}
        switch (CurrentTurnState)
        {
			case TurnState.PlayerMovement:
				Player.instance.currentHex.RevertAdjacentHighlights();
				MakeEnemiesMove();
				break;
			case TurnState.EnemyMovement:
				EnterEnemyActionTurn();
				break;
			case TurnState.EnemyAction:
				EnterPlayerAnswerTurn();
				break;
			case TurnState.PlayerAnswer:
				EndTurn();
				break;
			case TurnState.Loot:
				CycleTurn();
				break;
        }
    }

	void EnterLootTurn()
	{
		pendingLootTurn = false;
		CurrentTurnState = TurnState.Loot;
		if (LootPanel.GetRemainingItemCount() > 0)
		{
			LootPanel.gameObject.SetActive(true);
			LootPanel.Fill(3);
			TurnProgressButton.interactable = false;
		}
		else
		{
			ProgressTurn();
		}
	}

	void CycleTurn()
	{
		if (loadLevelAtTurnEnd)
		{
			loadLevelAtTurnEnd = false;
			pendingNewLevel = false;
			StartCoroutine(LoadNextLevel());
			return;
		}

		EnterPlayerMoveTurn();
		UpdateTurnText();
	}

	protected virtual IEnumerator LoadNextLevel(bool isFirstLevel = false)
	{
		if (loadedLevel != null)
		{
			Destroy(loadedLevel);
		}
		yield return null;
		currentLevel++;
		loadedLevel = Instantiate(levels[currentLevel]);
		if (currentLevel == levels.Length - 1)
		{
			isLastLevel = true;
		}
		GameObject startHexHook = GameObject.FindGameObjectWithTag("start hex");
		Hex startHex = startHexHook.transform.parent.GetComponent<Hex>();
		Player.instance.currentHex = startHex;
		Player.instance.transform.position = startHex.transform.position + Hex.posOffset;
		startHex.isOccupiedByPlayer = true;

		EnemyManager.instance.RegisterFirstEnemies();

		turnCount = turnLimitForNewWave;
		UpdateTurnCountText();
		currentWave = 0;

		if (!isFirstLevel)
		{
			EnterPlayerMoveTurn();
			UpdateTurnText();
		}
	}

	protected virtual void EnterPlayerAnswerTurn()
	{
		SidestepSkillButton.gameObject.SetActive(true);
		SkillCanvas.HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
		if (isJumpUnlocked)
		{
			JumpSkillButton.gameObject.SetActive(true);
			SkillCanvas.HandleButtonIconsForSkill(Skill.Jump, SkillType.None, JumpSkillButton);
		}
		CurrentTurnState = TurnState.PlayerAnswer;
		UpdateTurnText();

		
		UpdateUnansweredEnemyText();
		unansweredEnemyText.gameObject.SetActive(true);
	}

    protected IEnumerator ProcessCombat()
    {
		for (int i = 0; i < EnemyManager.instance.Enemies.Count; i++)
		{
			Enemy enemy = EnemyManager.instance.Enemies[i];
			if (!enemy.answeredThisTurn && enemy.CurrentAction != null)
			{
				Skill.HandleClash(enemy, null);
				yield return StartCoroutine(HandleClashAnimations(new List<Enemy>{ enemy }, null));
			}
		}

        int killedEnemyCount = EnemyManager.instance.KillMarkedEnemies(out bool willSendNewWave, ref pendingLootTurn, ref killsUntilNextItem, ref pendingNewLevel, ref turnCount, ref currentWave, ref isLastLevel, ref loadLevelAtTurnEnd);
		Resource rechargeStyleModifier = new Resource();
		if (killedEnemyCount > 1)
		{
			rechargeStyleModifier += 1;
		}
		if (!IsGameOver)
		{
			UpdateTurnCountText();
			int rechargeMultiplier = willSendNewWave ? (turnCount) : 1;
			rechargeMultiplier = Mathf.Max(rechargeMultiplier, 1);
			Player.instance.RechargeResources(rechargeStyleModifier, rechargeMultiplier, loadLevelAtTurnEnd);
			EnemyManager.instance.ResetEnemies();
			TurnProgressButton.interactable = true;
			if (pendingLootTurn)
			{
				EnterLootTurn();
			}
			else
			{
				CycleTurn();
			}
		}
    }

	IEnumerator HandleClashAnimations(List<Enemy> enemies, Skill playerAnswer)
	{
		isIngameInputActive = false;
		TurnProgressButton.interactable = false;
		if (playerAnswer == Skill.Skewer
				|| playerAnswer == Skill.Whirlwind
				|| playerAnswer == Skill.LightningReflexes)
		{
			// TODO: maybe merge cases?
			for (int i = 0; i < enemies.Count; i++)
			{
				Enemy enemy = enemies[i];
				bool playerShouldFaceLeft = enemy.transform.position.x < Player.instance.transform.position.x;
				Player.instance.SetRendererFlip(playerShouldFaceLeft);
				enemy.SetRendererFlip(!playerShouldFaceLeft);
				Player.instance.animator.Play(playerAnswer.clip);
				if (enemy.CurrentAction != null)
				{
					enemy.animator.Play(enemy.CurrentAction.clip);
				}
			}

			yield return new WaitForSeconds(1.5f);
			isIngameInputActive = true;
			if (!IsGameOver)
			{
				TurnProgressButton.interactable = true;
			}
		}
		else
		{
			Enemy enemy = enemies[0];
			Skill enemyAction = enemy.CurrentAction;

			bool playerShouldFaceLeft = enemy.transform.position.x < Player.instance.transform.position.x;
			Player.instance.SetRendererFlip(playerShouldFaceLeft);
			enemy.SetRendererFlip(!playerShouldFaceLeft);

			if (enemy.IsDefensive() && playerAnswer == null)
			{
				isIngameInputActive = true;
				if (!IsGameOver)
				{
					TurnProgressButton.interactable = true;
				}
				yield break;
			}

			if (playerAnswer != null)
			{
				Player.instance.animator.Play(playerAnswer.clip);
			}

			if (enemyAction != null)
			{
				enemy.animator.Play(enemyAction.clip);
			}

			//if (enemyAction != Skill.ShootArrow)
			//{
				//actionCamera.SetActive(true);
				//actionCanvas.SetActive(true);
				//Vector3 actionCameraPos = (enemy.transform.position + Player.instance.transform.position) / 2f;
				//actionCameraPos.z = -10f;
				//actionCamera.transform.position = actionCameraPos;
			//}


			// move creatures for clash
			if (enemyAction != Skill.ShootArrow && playerAnswer != Skill.Hook && playerAnswer != Skill.Heartshot
					&& enemyAction != Skill.Skewer)
			{
				Vector3 basePos = (enemy.IsDefensive() || enemyAction == null) ? enemy.transform.position : Player.instance.transform.position;
				Vector3 offset = playerShouldFaceLeft ? new Vector3(-0.3f, 0f, 0f) : new Vector3(0.3f, 0f, 0f);
				enemy.transform.position = basePos + offset;
				Player.instance.transform.position = basePos - offset;
			}

			StartCoroutine(ShakeScreenWithDelay(0.7f));


			yield return new WaitForSeconds(1.5f);

			//actionCamera.SetActive(false);
			//actionCanvas.SetActive(false);

			if (playerAnswer == Skill.Hook)
			{
				enemy.MoveToHex(HexHelper.GetNewHexForHookedEnemy(enemy), true);
			}
			else if (playerAnswer == Skill.Shove)
			{
				enemy.MoveToHex(HexHelper.GetNewHexForShovedEnemy(enemy), true);
			}
			else
			{
				enemy.transform.position = enemy.currentHex.transform.position + Hex.posOffset;
			}

			Player.instance.transform.position = Player.instance.currentHex.transform.position + Hex.posOffset;

			isIngameInputActive = true;
			if (!IsGameOver)
			{
				TurnProgressButton.interactable = true;
			}
		}
	}

	IEnumerator ShakeScreenWithDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		Camera.main.DOShakePosition(0.2f, 0.06f, 20, 60f, false);
	}

    public void OnPlayAgainClicked()
    {
        SceneManager.LoadScene("game");
    }

	public void OnYesClicked_UnansweredEnemiesPanel()
	{
		unansweredEnemiesPanel.SetActive(false);
		EndTurn(true);
	}

	public void OnYesDontShowAgainClicked_UnansweredEnemiesPanel()
	{
		unansweredEnemiesPanel.SetActive(false);
		showUnansweredEnemiesPanel = false;
		PlayerPrefs.SetInt("showUnansweredPanel", 0);
		PlayerPrefs.Save();
		EndTurn(true);
	}

	public void OnNoClicked_UnansweredEnemiesPanel()
	{
		unansweredEnemiesPanel.SetActive(false);
		TurnProgressButton.interactable = true;
	}

    protected virtual void EndTurn(bool forced = false)
    {
		if (!forced && EnemyManager.instance.GetUnansweredEnemyCount() > 0 && showUnansweredEnemiesPanel)
		{
			unansweredEnemiesPanel.SetActive(true);
			TurnProgressButton.interactable = false;
			return;
		}
		SkillsParent.SetActive(false);
		SidestepSkillButton.gameObject.SetActive(false);
		JumpSkillButton.gameObject.SetActive(false);
		Player.instance.currentHex.RevertAdjacentHighlights();
		unansweredEnemyText.gameObject.SetActive(false);
		TurnProgressButton.interactable = false;
		if (CurrentEnemy != null)
		{
			CurrentEnemy.currentHex.UnselectAsTarget();
		}
		if (enemyBeingCharged != null)
		{
			enemyBeingCharged.currentHex.SetAffectedBySkill(false);
		}
		UpdateTurnText();
        StartCoroutine(ProcessCombat());
    }

    protected void EnterEnemyActionTurn()
    {
        CurrentTurnState = TurnState.EnemyAction;
		UpdateTurnText();
		EnemyManager.instance.PickActionForEnemies();
		ProgressTurn();
    }

    public bool IsCurrentEnemyVulnerable()
    {
        return CurrentEnemy.IsVulnerable;
    }

    public virtual void OnEnemyClicked(Enemy enemy)
    {
        if (CurrentTurnState == TurnState.PlayerAnswer)
        {
			SidestepSkillButton.gameObject.SetActive(false);
			JumpSkillButton.gameObject.SetActive(false);
			
			Player.instance.currentHex.RevertAdjacentHighlights();

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

			
			bool isEnemyShootingArrow = CurrentEnemy.CurrentAction == Skill.ShootArrow;
			bool isEnemySkewering = CurrentEnemy.CurrentAction == Skill.Skewer;
			bool isEnemyDefensive = CurrentEnemy.IsDefensive();
			bool isEnemyVulnerable = CurrentEnemy.IsVulnerable;
			bool isAdjacentToEnemy = CurrentEnemy.currentHex.IsAdjacentToPlayer();
			bool isEnemyIdle = CurrentEnemy.CurrentAction == null;
			bool hasLos = CurrentEnemy.HasLosToPlayer(CurrentEnemy.currentHex);
			bool wrestleUsed = Player.instance.wrestleUsed;
			bool canSkewer = GetAnsweredEnemiesBySkill(Skill.Skewer).Contains(CurrentEnemy);
			bool chargeUsed = Player.instance.chargeUsed;
			bool gap = Player.instance.currentHex.HasGapBetweenHex(CurrentEnemy.currentHex);
			bool isEnemyAnswered = CurrentEnemy.answeredThisTurn;

			SkillType enemyActionType = isEnemyIdle ? SkillType.None : CurrentEnemy.CurrentAction.Type;

			SkillCanvas.instance.HandleSkills(isEnemyShootingArrow, isEnemySkewering, isEnemyDefensive, isEnemyVulnerable, isAdjacentToEnemy, isEnemyIdle, hasLos, wrestleUsed, canSkewer, chargeUsed, gap, enemyActionType, isEnemyAnswered);
        }
    }

    public void OnPlayerClicked()
    {

    }

    public void OnProgressTurnClicked()
    {
        ProgressTurn();
    }

    public void OnMouseButtonDownOnSkill()
    {
        IngameInput.instance.clickingButton = true;
    }

	// 無駄無駄無駄無駄無駄無駄無駄無駄無駄
	// 無駄~
	Enemy GetEnemyBeingChargedAt()
	{
		Ray ray = new Ray(Player.instance.currentHex.transform.position, CurrentEnemy.currentHex.transform.position - Player.instance.currentHex.transform.position);
		float distance = Vector3.Distance(Player.instance.currentHex.transform.position, CurrentEnemy.currentHex.transform.position);
		RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, distance, 1 << 8);
		hits = hits.OrderBy(h => h.distance).ToArray();
		if (!hits[0].collider.CompareTag("Player"))
		{
			return hits[0].collider.GetComponent<Enemy>();
		}
		else
		{
			return hits[1].collider.GetComponent<Enemy>();
		}
	}

	public void OnMouseButtonEnterOnSkill(SkillType skillType)
	{
		if (skillType == SkillType.Wrestle)
		{
			CurrentEnemy.currentHex.SetAffectedBySkill(true);
		}
		else if (skillType == SkillType.Charge)
		{
			enemyBeingCharged = GetEnemyBeingChargedAt();
			enemyBeingCharged.currentHex.SetAffectedBySkill(true);
		}
		else
		{
			List<Enemy> enemies = GetAnsweredEnemiesBySkill(Skill.GetSkillForType(skillType));
			for (int i = 0; i < enemies.Count; i++)
			{
				enemies[i].currentHex.SetAffectedBySkill(true);
			}
		}
	}

	public void OnMouseButtonExitOnSkill(SkillType skillType)
	{
		if (skillType == SkillType.Wrestle)
		{
			CurrentEnemy.currentHex.SetAffectedBySkill(false);
		}
		else if (skillType == SkillType.Charge)
		{
			enemyBeingCharged.currentHex.SetAffectedBySkill(false);
		}
		else
		{
			List<Enemy> enemies = GetAnsweredEnemiesBySkill(Skill.GetSkillForType(skillType));
			for (int i = 0; i < enemies.Count; i++)
			{
				enemies[i].currentHex.SetAffectedBySkill(false);
			}
		}
	}

    public void HandlePlayerDeath()
    {
        IsGameOver = true;
        DeathPanel.SetActive(true);
        TurnProgressButton.interactable = false;
    }

    public virtual void OnEmptyClick()
    {
        SkillsParent.SetActive(false);
        if (CurrentEnemy != null)
        {
            CurrentEnemy.currentHex.UnselectAsTarget();
			CurrentEnemy.currentHex.SetAffectedBySkill(false);
        }
        CurrentEnemy = null;
		if (enemyBeingCharged != null)
		{
			enemyBeingCharged.currentHex.SetAffectedBySkill(false);
		}
		enemyBeingCharged = null;

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

			if (!Player.instance.jumpUsed)
			{
				if (isJumpUnlocked)
				{
					JumpSkillButton.gameObject.SetActive(true);
					SkillCanvas.HandleButtonIconsForSkill(Skill.Jump, SkillType.None, JumpSkillButton);
				}
			}
			else
			{
				JumpSkillButton.gameObject.SetActive(false);
			}
			Player.instance.currentHex.RevertAdjacentHighlights();
			isSidestepActive = false;
			if (isJumpActive)
			{
				Player.instance.currentHex.RevertHightlightValidAdjacentsWithRange(2);
				isJumpActive = false;
			}
		}
    }

    public virtual void RegisterPlayerAction(Skill reaction, int damage)
    {
		List<Enemy> enemies = GetAnsweredEnemiesBySkill(reaction);

		if (reaction != null)
		{
			for (int i = 0; i < enemies.Count; i++)
			{
				Skill.HandleClash(enemies[i], reaction);
				enemies[i].SetPlayerReaction(reaction, damage);
				enemies[i].answeredThisTurn = true;
			}
			StartCoroutine(HandleClashAnimations(enemies, reaction));
		}
		UpdateUnansweredEnemyText();
		OnEmptyClick();
    }

	List<Enemy> GetAnsweredEnemiesBySkill(Skill skill)
	{
		List<Enemy> answeredEnemies = new List<Enemy>();
		if (skill == Skill.Skewer)
		{
			Ray ray = new Ray(Player.instance.currentHex.transform.position, CurrentEnemy.currentHex.transform.position - Player.instance.currentHex.transform.position);
			RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 2f, 1 << 8);
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].collider.CompareTag("Enemy"))
				{
					Enemy enemy = hits[i].collider.GetComponent<Enemy>();
					if (!enemy.answeredThisTurn && enemy.HasLosToPlayer(enemy.currentHex))
					{
						answeredEnemies.Add(enemy);
					}
				}
			}
		}
		else if (skill == Skill.Whirlwind)
		{
			Collider2D[] colliders = Physics2D.OverlapCircleAll(Player.instance.transform.position, 1f, 1 << 8);
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i].CompareTag("Enemy"))
				{
					Enemy enemy = colliders[i].GetComponent<Enemy>();
					if (!enemy.answeredThisTurn)
					{
						answeredEnemies.Add(enemy);
					}
				}
			}
		}
		else if (skill == Skill.Sidestep || skill == Skill.Wrestle)
		{
			return EnemyManager.instance.Enemies;
		}
		else if (skill == Skill.LightningReflexes)
		{
			for (int i = 0; i < EnemyManager.instance.Enemies.Count; i++)
			{
				Enemy enemy = EnemyManager.instance.Enemies[i];
				if (!enemy.answeredThisTurn && enemy.CurrentAction == Skill.ShootArrow)
				{
					answeredEnemies.Add(enemy);
				}
			}
		}
		else
		{
			answeredEnemies.Add(CurrentEnemy);
		}

		return answeredEnemies;
	}

    public void OnHowToPlayClicked()
    {
		tutorialCanvas.SetActive(true);
		TurnProgressButton.interactable = false;
		isHowToPlayActive = true;
    }

	public void OnBackToGameClicked()
	{
		tutorialCanvas.SetActive(false);
		isHowToPlayActive = false;
		if (!IsGameOver && (CurrentTurnState == TurnState.PlayerMovement || CurrentTurnState == TurnState.PlayerAnswer))
		{
			TurnProgressButton.interactable = true;
		}
	}

    public void OnQuitClicked()
    {
        Application.Quit();
    }

	public void OnSkillUnlocked(SkillType skillType)
	{
		switch (skillType)
		{
			case SkillType.Skewer:
				isSkewerUnlocked = true;
				break;
			case SkillType.BlockArrow:
				isBlockArrowUnlocked = true;
				break;
			case SkillType.Whirlwind:
				isWhirlwindUnlocked = true;
				break;
			case SkillType.Hook:
				isHookUnlocked = true;
				break;
			case SkillType.Wrestle:
				isWrestleUnlocked = true;
				break;
			case SkillType.Heartshot:
				isHeartshotUnlocked = true;
				break;
			case SkillType.LightningReflexes:
				isLightningReflexesUnlocked = true;
				break;
			case SkillType.Charge:
				isChargeUnlocked = true;
				break;
			case SkillType.Jump:
				isJumpUnlocked = true;
				break;
			default:
				break;
		}
	}

	public void OnItemClicked(Item item)
	{
		LootPanel.OnItemPicked(item);
		if (buildingCharacter)
		{
			characterBuild++;
			if (characterBuild == 3)
			{
				LootPanel.gameObject.SetActive(false);
				TurnProgressButton.interactable = true;
				Inventory.instance.SetInventoryActive(false);
				buildingCharacter = false;
				ProgressTurn();
			}
			else
			{
				LootPanel.Fill(2);
			}
		}
		else
		{
			TurnProgressButton.interactable = true;
			LootPanel.gameObject.SetActive(false);
			ProgressTurn();
		}
	}
}

public enum TurnState
{
	PlayerMovement,	// player moves, player resolve
	EnemyMovement,	// enemies move, auto resolve
	EnemyAction,	// enemy actions appear, auto resolve
	PlayerAnswer,	// player answers, player resolve
	Loot			// new items. resolved when item is picked. this state is only reached after wave is cleared
}
