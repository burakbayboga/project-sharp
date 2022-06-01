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
	public GameObject[] splatterPrefabs;

    public TextMeshProUGUI TurnStateText;
    public GameObject SkillsParent;
	public GameObject unansweredEnemiesPanel;

	public LootPanel lootPanel;
	public GameObject actionCamera;
	public GameObject actionCanvas;
	public GameObject tutorialCanvas;

	public bool isHowToPlayActive;
	public bool isTutorialPanelActive;
    
    public SkillButton SwiftAttackSkillButton;
    public SkillButton HeavyAttackSkillButton;
    public SkillButton BlockSkillButton;
    public SkillButton CounterSkillButton;
    public SkillButton KillingBlowSkillButton;
	public SkillButton DeflectArrowSkillButton;
	public SkillButton SkewerSkillButton;
	public SkillButton BlockArrowSkillButton;
	public SkillButton WhirlwindSkillButton;
	public SkillButton SidestepSkillButton;
	public SkillButton HookSkillButton;
	public SkillButton WrestleSkillButton;
	public SkillButton ShoveSkillButton;
	public SkillButton HeartshotSkillButton;
	public SkillButton LightningReflexesSkillButton;
	public SkillButton ResetSkillButton;
	public SkillButton ChargeSkillButton;

	// TODO: skill unlock system
	bool isSkewerUnlocked;
	bool isBlockArrowUnlocked;
	bool isWhirlwindUnlocked;
	bool isHookUnlocked;
	bool isWrestleUnlocked;
	bool isHeartshotUnlocked;
	bool isLightningReflexesUnlocked;
	bool isChargeUnlocked;

	bool showUnansweredEnemiesPanel;

    public Button TurnProgressButton;
	public Text unansweredEnemyText;
	public Text turnCountText;
	int turnCount;
	int turnLimitForNewWave = 10;
	protected int waveLimitForLevel;

	int killsRequiredForNewItem = 4;
	int killsUntilNextItem;

    public GameObject DeathPanel;

    public GameObject BloodEffectPrefab;

    List<Clash> Clashes = new List<Clash>();
	protected List<Enemy> Enemies = new List<Enemy>();
    protected List<Enemy> EnemiesMarkedForDeath = new List<Enemy>();

    protected Enemy CurrentEnemy;

	Enemy enemyBeingCharged;

    public TurnState CurrentTurnState;

    public bool IsGameOver;

	public bool isSidestepActive = false;

	protected int currentLevel = -1;

	bool pendingLootTurn;
	public int currentWave = 0;
	bool pendingNewLevel;
	bool loadLevelAtTurnEnd;
	int characterBuild;
	bool buildingCharacter;
	protected bool isLastLevel;

	protected GameObject loadedLevel;

    void Awake()
    {
        instance = this;
        IsGameOver = false;
    }

    public virtual void Start()
    {
		waveLimitForLevel = 2;
        CurrentTurnState = TurnState.Loot;
		UpdateTurnText();
		turnCount = turnLimitForNewWave;
		UpdateTurnCountText();

		StartCoroutine(LoadNextLevel(true));

        Skill.InitSkills();
		lootPanel.Init();
		killsUntilNextItem = killsRequiredForNewItem;

		StartCharacterBuild();

		showUnansweredEnemiesPanel = PlayerPrefs.GetInt("showUnansweredPanel", 1) == 1;
    }

	void StartCharacterBuild()
	{
		TurnProgressButton.interactable = false;
		buildingCharacter = true;
		characterBuild = 0;
		lootPanel.Fill(2);
		lootPanel.gameObject.SetActive(true);
		Inventory.instance.SetInventoryActive(true);
	}

	protected virtual void UpdateTurnCountText()
	{
		turnCountText.text = turnCount.ToString();
	}

	protected void UpdateTurnText()
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
			case TurnState.Clash:
				return "Clash";
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

	public void RegisterNewEnemies(List<Enemy> newEnemies)
	{
		Enemies.AddRange(newEnemies);
		UpdateUnansweredEnemyText();
	}

	int GetUnansweredEnemyCount()
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

	protected void UpdateUnansweredEnemyText()
	{
		unansweredEnemyText.text = GetUnansweredEnemyCount().ToString() + " Enemies\nNot Answered";
	}

	protected virtual void MakeEnemiesMove()
	{
		// TODO: should enemies even move?
		CurrentTurnState = TurnState.EnemyMovement;
		UpdateTurnText();

		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].MoveTurn();
		}
        
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
			Resource resourceGivenBack = GetResourceSpentOnCurrentEnemy(Skill.Sidestep);
			Player.instance.OnPlayerSidestep(resourceGivenBack);
			while (Clashes.Count > 0)
			{
				EraseClash(Clashes[0]);
			}
			isSidestepActive = false;
			SidestepSkillButton.gameObject.SetActive(false);
			for (int i = 0; i < Enemies.Count; i++)
			{
				Enemies[i].CheckActionValidity();
			}
			
			UpdateUnansweredEnemyText();
		}
	}

	Hex GetNewHexForChargingPlayer(Hex targetHex)
	{
		Hex traverse = Player.instance.currentHex;
		float realStartTime = Time.realtimeSinceStartup;
		while (true)
		{
			if (Time.realtimeSinceStartup - realStartTime > 10f)
			{
				Debug.LogError("new hex for charging player safety net");
				break;
			}

			Hex next;
			Ray ray = new Ray(traverse.transform.position, targetHex.transform.position - traverse.transform.position);
			RaycastHit2D[] hits = Physics2D.CircleCastAll(ray.origin, 0.25f, ray.direction, 0.5f, 1 << 9);
			Hex nextCandidate = GetSuitableHexForCreatureFromHits(hits, traverse);
			if (nextCandidate != null)
			{
				next = nextCandidate;
				if (next == targetHex)
				{
					break;
				}
				else
				{
					traverse = next;
				}
			}
			else
			{
				break;
			}
		}

		return traverse;
	}

	public void OnCharge()
	{
		while (Clashes.Count > 0)
		{
			EraseClash(Clashes[0]);
		}

		Hex hex = GetNewHexForChargingPlayer(CurrentEnemy.currentHex);
		Player.instance.MovePlayer(hex);
		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].CheckActionValidity();
		}
		enemyBeingCharged.ForceCancelAction();
		OnEmptyClick();
	}

	public void OnWrestle()
	{
		while (Clashes.Count > 0)
		{
			EraseClash(Clashes[0]);
		}

		CurrentEnemy.currentHex.SetAffectedBySkill(false);
		Hex playerHex = Player.instance.currentHex;
		Player.instance.MovePlayer(CurrentEnemy.currentHex, false, true);
		CurrentEnemy.MoveToHex(playerHex, true);

		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].CheckActionValidity();
		}
		//CurrentEnemy.ForceCancelAction();
		UpdateUnansweredEnemyText();
		OnEmptyClick();
	}

	public bool IsAggressiveEnemyAdjacentToPlayer()
	{
		Hex[] adjacents = Player.instance.currentHex.adjacents;
		for (int i = 0; i < adjacents.Length; i++)
		{
			if (adjacents[i].enemy != null
					&& adjacents[i].enemy.CurrentAction != null
					&& !adjacents[i].enemy.IsDefensive())
			{
				return true;
			}
		}

		return false;
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
			case TurnState.Clash:
				if (pendingLootTurn)
				{
					EnterLootTurn();
				}
				else
				{
					CycleTurn();
				}
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
		if (lootPanel.GetRemainingItemCount() > 0)
		{
			lootPanel.gameObject.SetActive(true);
			lootPanel.Fill(3);
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

		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
			Enemies.Add(enemies[i].GetComponent<Enemy>());
			Enemies[i].Init(Enemies[i].currentHex);	// wow
        }
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
		HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
		CurrentTurnState = TurnState.PlayerAnswer;
		UpdateTurnText();

		
		UpdateUnansweredEnemyText();
		unansweredEnemyText.gameObject.SetActive(true);
	}

    protected IEnumerator ProcessCombat()
    {
		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemy enemy = Enemies[i];
			if (enemy.CurrentClash == null && (enemy.CurrentAction != null || enemy.CurrentPlayerReaction != null))
			{
				Clash newClash = new Clash(new List<Enemy>{ enemy }, null, new Resource());
				enemy.CurrentClash = newClash;
				Clashes.Insert(0, newClash);
			}
		}

		for (int i = 0; i < Clashes.Count; i++)
		{
			Clash clash = Clashes[i];
			for (int j = 0; j < clash.enemies.Count; j++)
			{
				Enemy enemy = clash.enemies[j];
				Skill.HandleClash(enemy, clash.playerAnswer);
			}
			yield return StartCoroutine(HandleClashAnimations(clash));
		}

        int killedEnemyCount = KillMarkedEnemies();
		Resource rechargeStyleModifier = new Resource();
		if (killedEnemyCount > 1)
		{
			rechargeStyleModifier += 1;
		}
		if (!IsGameOver)
		{
			UpdateTurnCountText();
			Player.instance.RechargeResources(rechargeStyleModifier);
			ResetClashes();
			TurnProgressButton.interactable = true;
			ProgressTurn();
		}
    }

	IEnumerator HandleClashAnimations(Clash clash)
	{
		if (clash.playerAnswer == Skill.Skewer
				|| clash.playerAnswer == Skill.Whirlwind
				|| clash.playerAnswer == Skill.LightningReflexes)
		{
			// TODO: maybe merge cases?
			for (int i = 0; i < clash.enemies.Count; i++)
			{
				Enemy enemy = clash.enemies[i];
				bool playerShouldFaceLeft = enemy.transform.position.x < Player.instance.transform.position.x;
				Player.instance.SetRendererFlip(playerShouldFaceLeft);
				enemy.SetRendererFlip(!playerShouldFaceLeft);
				Player.instance.animator.Play(clash.playerAnswer.clip);
				if (enemy.CurrentAction != null)
				{
					enemy.animator.Play(enemy.CurrentAction.clip);
				}
			}

			yield return new WaitForSeconds(1.5f);
		}
		else
		{
			Enemy enemy = clash.enemies[0];
			Skill enemyAction = enemy.CurrentAction;
			Skill playerReaction = clash.playerAnswer;

			bool playerShouldFaceLeft = enemy.transform.position.x < Player.instance.transform.position.x;
			Player.instance.SetRendererFlip(playerShouldFaceLeft);
			enemy.SetRendererFlip(!playerShouldFaceLeft);

			if (enemy.IsDefensive() && playerReaction == null)
			{
				yield break;
			}

			if (playerReaction != null)
			{
				Player.instance.animator.Play(playerReaction.clip);
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
			if (enemyAction != Skill.ShootArrow && playerReaction != Skill.Hook && playerReaction != Skill.Heartshot
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

			if (playerReaction == Skill.Hook)
			{
				enemy.MoveToHex(GetNewHexForHookedEnemy(enemy), true);
			}
			else if (playerReaction == Skill.Shove)
			{
				enemy.MoveToHex(GetNewHexForShovedEnemy(enemy), true);
			}
			else
			{
				enemy.transform.position = enemy.currentHex.transform.position + Hex.posOffset;
			}

			Player.instance.transform.position = Player.instance.currentHex.transform.position + Hex.posOffset;
		}
	}

	IEnumerator ShakeScreenWithDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		Camera.main.DOShakePosition(0.2f, 0.06f, 20, 60f, false);
	}
	
	Hex GetNewHexForShovedEnemy(Enemy enemy)
	{
		Hex traverse = enemy.currentHex;
		Hex playerHex = Player.instance.currentHex;
		Vector3 direction = traverse.transform.position - playerHex.transform.position;

		for (int i = 0; i < 2; i++)
		{
			Ray ray = new Ray(traverse.transform.position, direction);
			RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 1f, 1 << 9);
			Hex next = GetSuitableHexForCreatureFromHits(hits, traverse);
			if (next != null)
			{
				traverse = next;
			}
			else
			{
				break;
			}
		}

		return traverse;
	}

	Hex GetNewHexForHookedEnemy(Enemy enemy)
	{
		Hex traverse = enemy.currentHex;
		Hex playerHex = Player.instance.currentHex;
		float realStartTime = Time.realtimeSinceStartup;
		while (true)
		{
			if (Time.realtimeSinceStartup - realStartTime > 10f)
			{
				Debug.LogError("new hex for hooked enemy safety net");
				// safety net
				break;
			}
			Hex next;
			Ray ray = new Ray(traverse.transform.position, playerHex.transform.position - traverse.transform.position);
			RaycastHit2D[] hits = Physics2D.CircleCastAll(ray.origin, 0.25f, ray.direction, 0.5f, 1 << 9);
			Hex nextCandidate = GetSuitableHexForCreatureFromHits(hits, traverse);
			if (nextCandidate != null)
			{
				next = nextCandidate;
				if (next.isOccupiedByPlayer)
				{
					break;
				}
				else
				{
					traverse = next;
				}
			}
			else
			{
				break;
			}
		}

		return traverse;
	}

	Hex GetSuitableHexForCreatureFromHits(RaycastHit2D[] hits, Hex baseHex)
	{
		for (int i = 0; i < hits.Length; i++)
		{
			Hex candidate = hits[i].collider.GetComponent<Hex>();
			if (candidate != baseHex && candidate.enemy == null)
			{
				return candidate;
			}
		}

		return null;
	}

    public void MarkEnemyForDeath(Enemy enemy)
    {
        EnemiesMarkedForDeath.Add(enemy);
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
		if (!forced && GetUnansweredEnemyCount() > 0 && showUnansweredEnemiesPanel)
		{
			unansweredEnemiesPanel.SetActive(true);
			TurnProgressButton.interactable = false;
			return;
		}
		SkillsParent.SetActive(false);
		SidestepSkillButton.gameObject.SetActive(false);
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
		CurrentTurnState = TurnState.Clash;
		UpdateTurnText();
        StartCoroutine(ProcessCombat());
    }

    protected virtual int KillMarkedEnemies()
    {
		int killedEnemyCount = EnemiesMarkedForDeath.Count;
        while (EnemiesMarkedForDeath.Count > 0)
        {
            Enemy temp = EnemiesMarkedForDeath[0];
            Enemies.Remove(temp);
            EnemiesMarkedForDeath.Remove(temp);
            Instantiate(BloodEffectPrefab, temp.transform.position, Quaternion.identity);
			GameObject splatter = Instantiate(splatterPrefabs[Random.Range(0, splatterPrefabs.Length)], temp.currentHex.transform.position, Quaternion.identity);
			splatter.transform.SetParent(loadedLevel.transform);
			temp.currentHex.enemy = null;
            Destroy(temp.gameObject);

			killsUntilNextItem--;
			if (killsUntilNextItem == 0)
			{
				pendingLootTurn = true;
				killsUntilNextItem = killsRequiredForNewItem;
				Player.instance.ResetInjuries();
			}
        }

		turnCount--;
        if (Enemies.Count == 0 || turnCount == 0)
        {
			if (currentWave % 2 == 0)
			{
				lootPanel.IncreaseItemQuality();
			}
			turnCount = turnLimitForNewWave;
			UpdateTurnCountText();
			if (currentWave == waveLimitForLevel && isLastLevel)
			{
				print("now entering endless waves");
			}
			if (currentWave == waveLimitForLevel && !isLastLevel)
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
				WaveManager.instance.SendNewWave();
				currentWave++;
			}
        }

		return killedEnemyCount;
    }

    void ResetClashes()
    {
		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].ResetEnemy();
		}
		Clashes.Clear();
    }

    protected void EnterEnemyActionTurn()
    {
        CurrentTurnState = TurnState.EnemyAction;
		UpdateTurnText();

		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].PickAction();
		}

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

			SkillType enemyActionType = isEnemyIdle ? SkillType.None : CurrentEnemy.CurrentAction.Type;

			List<SkillButton> availableSkills = new List<SkillButton>();
			SkillButton availableKillingBlow = null;

			if ((!isEnemyShootingArrow && !isEnemyDefensive && isAdjacentToEnemy && !isEnemyIdle) || isEnemySkewering)
			{
				BlockSkillButton.gameObject.SetActive(true);
				availableSkills.Add(BlockSkillButton);
				HandleButtonIconsForSkill(Skill.Block, enemyActionType, BlockSkillButton);
			}
			else
			{
				BlockSkillButton.gameObject.SetActive(false);
			}
			if ((!isEnemyShootingArrow && !isEnemyDefensive && isAdjacentToEnemy && !isEnemyIdle) || isEnemySkewering)
			{
				CounterSkillButton.gameObject.SetActive(true);
				availableSkills.Add(CounterSkillButton);
				HandleButtonIconsForSkill(Skill.Counter, enemyActionType, CounterSkillButton);
			}
			else
			{
				CounterSkillButton.gameObject.SetActive(false);
			}
			if (!isEnemyVulnerable && isAdjacentToEnemy)
			{
				SwiftAttackSkillButton.gameObject.SetActive(true);
				availableSkills.Add(SwiftAttackSkillButton);
				HandleButtonIconsForSkill(Skill.SwiftAttack, enemyActionType, SwiftAttackSkillButton);
			}
			else
			{
				SwiftAttackSkillButton.gameObject.SetActive(false);
			}
			if (!isEnemyShootingArrow && !isEnemyVulnerable && isAdjacentToEnemy)
			{
				HeavyAttackSkillButton.gameObject.SetActive(true);
				availableSkills.Add(HeavyAttackSkillButton);
				HandleButtonIconsForSkill(Skill.HeavyAttack, enemyActionType, HeavyAttackSkillButton);
			}
			else
			{
				HeavyAttackSkillButton.gameObject.SetActive(false);
			}
			if (isSkewerUnlocked && canSkewer)
			{
				SkewerSkillButton.gameObject.SetActive(true);
				availableSkills.Add(SkewerSkillButton);
				HandleButtonIconsForSkill(Skill.Skewer, enemyActionType, SkewerSkillButton);
			}
			else
			{
				SkewerSkillButton.gameObject.SetActive(false);
			}
			if (isEnemyShootingArrow)
			{
				DeflectArrowSkillButton.gameObject.SetActive(true);
				availableSkills.Add(DeflectArrowSkillButton);
				HandleButtonIconsForSkill(Skill.DeflectArrow, enemyActionType, DeflectArrowSkillButton);
			}
			else
			{
				DeflectArrowSkillButton.gameObject.SetActive(false);
			}
			if (isLightningReflexesUnlocked && isEnemyShootingArrow)
			{
				LightningReflexesSkillButton.gameObject.SetActive(true);
				availableSkills.Add(LightningReflexesSkillButton);
				HandleButtonIconsForSkill(Skill.LightningReflexes, enemyActionType, LightningReflexesSkillButton);
			}
			else
			{
				LightningReflexesSkillButton.gameObject.SetActive(false);
			}
			if (isBlockArrowUnlocked && isEnemyShootingArrow)
			{
				BlockArrowSkillButton.gameObject.SetActive(true);
				availableSkills.Add(BlockArrowSkillButton);
				HandleButtonIconsForSkill(Skill.BlockArrow, enemyActionType, BlockArrowSkillButton);
			}
			else
			{
				BlockArrowSkillButton.gameObject.SetActive(false);
			}
			if (isWhirlwindUnlocked && isAdjacentToEnemy)
			{
				WhirlwindSkillButton.gameObject.SetActive(true);
				availableSkills.Add(WhirlwindSkillButton);
				HandleButtonIconsForSkill(Skill.Whirlwind, enemyActionType, WhirlwindSkillButton);
			}
			else
			{
				WhirlwindSkillButton.gameObject.SetActive(false);
			}
			if (isHookUnlocked && !isAdjacentToEnemy && hasLos)
			{
				HookSkillButton.gameObject.SetActive(true);
				availableSkills.Add(HookSkillButton);
				HandleButtonIconsForSkill(Skill.Hook, enemyActionType, HookSkillButton);
			}
			else
			{
				HookSkillButton.gameObject.SetActive(false);
			}
			if (isWrestleUnlocked && isAdjacentToEnemy && !wrestleUsed)
			{
				WrestleSkillButton.gameObject.SetActive(true);
				availableSkills.Add(WrestleSkillButton);
				HandleButtonIconsForSkill(Skill.Wrestle, enemyActionType, WrestleSkillButton);
			}
			else
			{
				WrestleSkillButton.gameObject.SetActive(false);
			}
			if (isAdjacentToEnemy)
			{
				ShoveSkillButton.gameObject.SetActive(true);
				availableSkills.Add(ShoveSkillButton);
				HandleButtonIconsForSkill(Skill.Shove, enemyActionType, ShoveSkillButton);
			}
			else
			{
				ShoveSkillButton.gameObject.SetActive(false);
			}
			if (isAdjacentToEnemy)
			{
				KillingBlowSkillButton.gameObject.SetActive(true);
				availableKillingBlow = KillingBlowSkillButton;
				HandleButtonIconsForSkill(Skill.KillingBlow, enemyActionType, KillingBlowSkillButton, isEnemyVulnerable);
			}
			else
			{
				KillingBlowSkillButton.gameObject.SetActive(false);
			}
			if (isHeartshotUnlocked && !isAdjacentToEnemy && hasLos)
			{
				HeartshotSkillButton.gameObject.SetActive(true);
				availableKillingBlow = HeartshotSkillButton;
				HandleButtonIconsForSkill(Skill.Heartshot, enemyActionType, HeartshotSkillButton, isEnemyVulnerable);
			}
			else
			{
				HeartshotSkillButton.gameObject.SetActive(false);
			}
			if (!isAdjacentToEnemy && isChargeUnlocked && !chargeUsed && hasLos)
			{
				ChargeSkillButton.gameObject.SetActive(true);
				availableSkills.Add(ChargeSkillButton);
				HandleButtonIconsForSkill(Skill.Charge, enemyActionType, ChargeSkillButton);
			}

			HandleSkillCanvas(availableSkills, availableKillingBlow);
        }
    }

	protected void HandleSkillCanvas(List<SkillButton> availableSkills, SkillButton availableKillingBlow)
	{
		float radius = 1.7f;
		float increment = 30f * Mathf.Deg2Rad;
		float theta = 90f * Mathf.Deg2Rad;
		int i = 0;

		Vector3 pos;
		for (; i < availableSkills.Count; i++)
		{
			pos = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f) * radius;
			availableSkills[i].SetPosition(pos);
			//if (i == 6)
			//{
				//break;
			//}
			theta -= increment;
		}
		ResetSkillButton.SetPosition(new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f) * radius);
		if (availableKillingBlow != null)
		{
			//float angle = 90f * Mathf.Deg2Rad + increment;
			//availableKillingBlow.SetPosition(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
			availableKillingBlow.SetPosition(new Vector3(-radius, 0f, 0f));
		}

		theta = 90f * Mathf.Deg2Rad - increment / 2f;
		radius *= 1.7f;
		for (; i < availableSkills.Count; i++)
		{
			pos = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f) * radius;
			availableSkills[i].SetPosition(pos);
			theta -= increment;
		}
	}

	// TODO: fix redundancy
    public Resource GetResourceSpentOnCurrentEnemy(Skill newSkill)
    {
		List<Enemy> answeredEnemies = GetAnsweredEnemiesBySkill(newSkill);
		List<Clash> affectedClashes = GetDistinctClashesFromEnemies(answeredEnemies);
		Resource totalSpent = new Resource();
		for (int i = 0; i < affectedClashes.Count; i++)
		{
			totalSpent += affectedClashes[i].resourceSpent;
		}
		return totalSpent;
    }

	List<Clash> GetDistinctClashesFromEnemies(List<Enemy> enemies)
	{
		List<Clash> clashes = new List<Clash>();
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].CurrentClash != null && !clashes.Contains(enemies[i].CurrentClash))
			{
				clashes.Add(enemies[i].CurrentClash);
			}
		}

		return clashes;
	}

	protected void HandleButtonIconsForSkill(Skill skill, SkillType enemyActionType, SkillButton skillButton, bool setKillingBlowIndicator = false)
	{
		Resource cost = skill.GetTotalCost(enemyActionType);
		int damage = skill.GetDamageAgainstEnemyAction(Skill.GetSkillForType(enemyActionType));
		skillButton.HandleCostAndDamage(cost, damage, setKillingBlowIndicator);
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
        IngameInput.instance.IsIngameInputActive = false;
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
				HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
			}
			else
			{
				SidestepSkillButton.gameObject.SetActive(false);
			}
			Player.instance.currentHex.RevertAdjacentHighlights();
			isSidestepActive = false;
		}
    }

    public virtual void RegisterPlayerAction(Skill reaction, int damage, Resource skillCost)
    {
		List<Enemy> enemies = GetAnsweredEnemiesBySkill(reaction);
		List<Clash> clashes = GetDistinctClashesFromEnemies(enemies);

		for (int i = 0; i < clashes.Count; i++)
		{
			EraseClash(clashes[i]);
		}

		if (reaction != null)
		{
			Clash newClash = new Clash(enemies, reaction, skillCost);
			for (int i = 0; i < enemies.Count; i++)
			{
				enemies[i].SetPlayerReaction(reaction, damage);
				enemies[i].CurrentClash = newClash;
			}
			Clashes.Add(newClash);

		}
		UpdateUnansweredEnemyText();
		OnEnemyClicked(CurrentEnemy);
    }

	void EraseClash(Clash clash)
	{
		UpdateUnansweredEnemyText();
		for (int i = 0; i < clash.enemies.Count; i++)
		{
			clash.enemies[i].SetPlayerReaction(null, 0);
			clash.enemies[i].CurrentClash = null;
		}
		Clashes.Remove(clash);
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
					if (enemy.HasLosToPlayer(enemy.currentHex))
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
					answeredEnemies.Add(colliders[i].GetComponent<Enemy>());
				}
			}
		}
		else if (skill == Skill.Sidestep || skill == Skill.Wrestle)
		{
			return Enemies;
		}
		else if (skill == Skill.LightningReflexes)
		{
			for (int i = 0; i < Enemies.Count; i++)
			{
				if (Enemies[i].CurrentAction == Skill.ShootArrow)
				{
					answeredEnemies.Add(Enemies[i]);
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
			default:
				break;
		}
	}

	public void OnItemClicked(Item item)
	{
		lootPanel.OnItemPicked(item);
		if (buildingCharacter)
		{
			characterBuild++;
			if (characterBuild == 3)
			{
				lootPanel.gameObject.SetActive(false);
				TurnProgressButton.interactable = true;
				Inventory.instance.SetInventoryActive(false);
				buildingCharacter = false;
				ProgressTurn();
			}
			else
			{
				lootPanel.Fill(2);
			}
		}
		else
		{
			TurnProgressButton.interactable = true;
			lootPanel.gameObject.SetActive(false);
			ProgressTurn();
		}
	}
}

public class Clash
{
	public Clash(List<Enemy> _enemies, Skill _playerAnswer, Resource _resourceSpent)
	{
		enemies = _enemies;
		playerAnswer = _playerAnswer;
		resourceSpent = _resourceSpent;
	}

	public List<Enemy> enemies;
    public Skill playerAnswer;
	public Resource resourceSpent;
}

public enum TurnState
{
	PlayerMovement,	// player moves, player resolve
	EnemyMovement,	// enemies move, auto resolve
	EnemyAction,	// enemy actions appear, auto resolve
	PlayerAnswer,	// player answers, player resolve
	Clash,			// handle clash, auto resolve
	Loot			// new items. resolved when item is picked. this state is only reached after wave is cleared
}
