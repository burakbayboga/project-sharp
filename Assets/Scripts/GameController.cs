using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class GameController : MonoBehaviour
{

    public static GameController instance;

	public GameObject[] levels;

    public Text TurnStateText;
    public GameObject SkillsParent;

	public LootPanel lootPanel;
	public GameObject actionCamera;
	public GameObject actionCanvas;
	public GameObject tutorialCanvas;

	public bool isTutorialActive;
    
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

	// TODO: skill unlock system
	bool isSkewerUnlocked;
	bool isBlockArrowUnlocked;
	bool isWhirlwindUnlocked;
	bool isHookUnlocked;
	bool isWrestleUnlocked;
	bool isHeartshotUnlocked;
	bool isLightningReflexesUnlocked;

    public Button TurnProgressButton;
	public Text unansweredEnemyText;
	public Text turnCountText;
	int turnCount;
	int turnLimitForNewWave = 10;

	int killsRequiredForNewItem = 4;
	int killsUntilNextItem;

    public GameObject DeathPanel;

    public GameObject BloodEffectPrefab;

    List<Clash> Clashes = new List<Clash>();
	List<Enemy> Enemies = new List<Enemy>();
    List<Enemy> EnemiesMarkedForDeath = new List<Enemy>();

    Enemy CurrentEnemy;

    public TurnState CurrentTurnState;

    public bool IsGameOver;

	public bool isSidestepActive = false;

	int currentLevel = -1;

	bool pendingLootTurn;
	public int currentWave = 0;
	bool pendingNewLevel;
	bool loadLevelAtTurnEnd;
	int characterBuild;
	bool buildingCharacter;
	bool isLastLevel;

	GameObject loadedLevel;

    void Awake()
    {
        instance = this;
        IsGameOver = false;
    }

    void Start()
    {
        CurrentTurnState = TurnState.Loot;
		UpdateTurnText();
		turnCount = 1;
		UpdateTurnCountText();

		StartCoroutine(LoadNextLevel(true));

        Skill.InitSkills();
		lootPanel.Init();
		killsUntilNextItem = killsRequiredForNewItem;

		StartCharacterBuild();
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

	void UpdateTurnCountText()
	{
		turnCountText.text = turnCount.ToString();
	}

	void UpdateTurnText()
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

	void UpdateUnansweredEnemyText()
	{
		unansweredEnemyText.text = GetUnansweredEnemyCount().ToString() + " Enemies\nNot Answered";
	}

	void MakeEnemiesMove()
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

	void EnterPlayerMoveTurn()
	{
		CurrentTurnState = TurnState.PlayerMovement;
		UpdateTurnText();
		Player.instance.currentHex.HighlightValidAdjacents();
	}

	public void OnPlayerMove()
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

	public void OnWrestle()
	{
		while (Clashes.Count > 0)
		{
			EraseClash(Clashes[0]);
		}

		CurrentEnemy.currentHex.SetAffectedBySkill(false);
		Hex playerHex = Player.instance.currentHex;
		Player.instance.MovePlayer(CurrentEnemy.currentHex);
		CurrentEnemy.MoveToHex(playerHex);

		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].CheckActionValidity();
		}
		//CurrentEnemy.ForceCancelAction();
		UpdateUnansweredEnemyText();
		OnEmptyClick();
	}

	// TODO: refactor plzz
	public bool IsAggressiveEnemyAdjacentToPlayer()
	{
		for (int i = 0; i < Enemies.Count; i++)
		{
			if (Enemies[i].currentHex.IsAdjacentToPlayer()
					&& Enemies[i].CurrentAction != null
					&& !Enemies[i].IsDefensive())
			{
				return true;
			}
		}

		return false;
	}

    void ProgressTurn()
    {
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

	IEnumerator LoadNextLevel(bool isFirstLevel = false)
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
		turnCount = 1;
		UpdateTurnCountText();
		currentWave = 0;

		if (!isFirstLevel)
		{
			EnterPlayerMoveTurn();
			UpdateTurnText();
		}
	}

	void EnterPlayerAnswerTurn()
	{
		SidestepSkillButton.gameObject.SetActive(true);
		HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
		CurrentTurnState = TurnState.PlayerAnswer;
		UpdateTurnText();

		
		UpdateUnansweredEnemyText();
		unansweredEnemyText.gameObject.SetActive(true);
	}

    IEnumerator ProcessCombat()
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


			Vector3 basePos = (enemy.IsDefensive() || enemyAction == null) ? enemy.transform.position : Player.instance.transform.position;
			// move creatures for clash
			if (enemyAction != Skill.ShootArrow && playerReaction != Skill.Hook)
			{
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
				enemy.MoveToHex(GetNewHexForHookedEnemy(enemy));
			}
			else if (playerReaction == Skill.Shove)
			{
				enemy.MoveToHex(GetNewHexForShovedEnemy(enemy));
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
			Hex next = GetSuitableHexForEnemyFromHits(hits, traverse);
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
			Hex nextCandidate = GetSuitableHexForEnemyFromHits(hits, traverse);
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

	Hex GetSuitableHexForEnemyFromHits(RaycastHit2D[] hits, Hex baseHex)
	{
		for (int i = 0; i < hits.Length; i++)
		{
			Hex candidate = hits[i].collider.GetComponent<Hex>();
			if (candidate != baseHex && !candidate.isOccupiedByEnemy)
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

    void EndTurn()
    {
		SidestepSkillButton.gameObject.SetActive(false);
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
    }

    int KillMarkedEnemies()
    {
		int killedEnemyCount = EnemiesMarkedForDeath.Count;
        while (EnemiesMarkedForDeath.Count > 0)
        {
            Enemy temp = EnemiesMarkedForDeath[0];
            Enemies.Remove(temp);
            EnemiesMarkedForDeath.Remove(temp);
            Instantiate(BloodEffectPrefab, temp.transform.position, Quaternion.identity);
			temp.currentHex.isOccupiedByEnemy = false;
            Destroy(temp.gameObject);

			killsUntilNextItem--;
			if (killsUntilNextItem == 0)
			{
				pendingLootTurn = true;
				killsUntilNextItem = killsRequiredForNewItem;
				Player.instance.ResetInjuries();
			}
        }

		turnCount++;
        if (Enemies.Count == 0 || turnCount == turnLimitForNewWave)
        {
			if (currentWave % 2 == 0)
			{
				lootPanel.IncreaseItemQuality();
			}
			turnCount = 1;
			UpdateTurnCountText();
			if (currentWave == 2 && isLastLevel)
			{
				print("now entering endless waves");
			}
			if (currentWave == 2 && !isLastLevel)
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
			Enemies[i].ResetIcons();
			Enemies[i].CurrentAction = null;
			Enemies[i].CurrentPlayerReaction = null;
			Enemies[i].CurrentClash = null;
		}
		Clashes.Clear();
    }

    void EnterEnemyActionTurn()
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

    public void OnEnemyClicked(Enemy enemy)
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

            HandleSkillButtonIcons();
			
			bool isEnemyShootingArrow = CurrentEnemy.CurrentAction == Skill.ShootArrow;
			bool isEnemyDefensive = CurrentEnemy.IsDefensive();
			bool isEnemyVulnerable = CurrentEnemy.IsVulnerable;
			bool isAdjacentToEnemy = CurrentEnemy.currentHex.IsAdjacentToPlayer();
			bool isEnemyIdle = CurrentEnemy.CurrentAction == null;
			bool hasLos = CurrentEnemy.HasLosToPlayer(CurrentEnemy.currentHex);
			bool wrestleUsed = Player.instance.wrestleUsed;

			BlockSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyDefensive && isAdjacentToEnemy && !isEnemyIdle);
			CounterSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyDefensive && isAdjacentToEnemy && !isEnemyIdle);
			SwiftAttackSkillButton.gameObject.SetActive(!isEnemyVulnerable && isAdjacentToEnemy);
			HeavyAttackSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyVulnerable && isAdjacentToEnemy);
			SkewerSkillButton.gameObject.SetActive(isSkewerUnlocked && isAdjacentToEnemy);
			DeflectArrowSkillButton.gameObject.SetActive(isEnemyShootingArrow);
			LightningReflexesSkillButton.gameObject.SetActive(isLightningReflexesUnlocked && isEnemyShootingArrow);
			BlockArrowSkillButton.gameObject.SetActive(isBlockArrowUnlocked && isEnemyShootingArrow);
			WhirlwindSkillButton.gameObject.SetActive(isWhirlwindUnlocked && isAdjacentToEnemy);
			HookSkillButton.gameObject.SetActive(isHookUnlocked && !isAdjacentToEnemy && hasLos);
			WrestleSkillButton.gameObject.SetActive(isWrestleUnlocked && isAdjacentToEnemy && !wrestleUsed);
			ShoveSkillButton.gameObject.SetActive(isAdjacentToEnemy);
			KillingBlowSkillButton.gameObject.SetActive(isAdjacentToEnemy);
			HeartshotSkillButton.gameObject.SetActive(isHeartshotUnlocked && !isAdjacentToEnemy);

			if (KillingBlowSkillButton.gameObject.activeSelf)
			{
				KillingBlowSkillButton.SetIndicatorAnimation(isEnemyVulnerable);
			}
			if (HeartshotSkillButton.gameObject.activeSelf)
			{
				HeartshotSkillButton.SetIndicatorAnimation(isEnemyVulnerable);
			}
        }
    }

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

    void HandleSkillButtonIcons()
    {
		SkillType enemyActionType = CurrentEnemy.CurrentAction != null ? CurrentEnemy.CurrentAction.Type : SkillType.None;

		HandleButtonIconsForSkill(Skill.HeavyAttack, enemyActionType, HeavyAttackSkillButton);
		HandleButtonIconsForSkill(Skill.SwiftAttack, enemyActionType, SwiftAttackSkillButton);
		HandleButtonIconsForSkill(Skill.Block, enemyActionType, BlockSkillButton);
		HandleButtonIconsForSkill(Skill.Counter, enemyActionType, CounterSkillButton);
		HandleButtonIconsForSkill(Skill.KillingBlow, enemyActionType, KillingBlowSkillButton);
		HandleButtonIconsForSkill(Skill.Heartshot, enemyActionType, HeartshotSkillButton);
		HandleButtonIconsForSkill(Skill.DeflectArrow, enemyActionType, DeflectArrowSkillButton);
		HandleButtonIconsForSkill(Skill.Skewer, enemyActionType, SkewerSkillButton);
		HandleButtonIconsForSkill(Skill.BlockArrow, enemyActionType, BlockArrowSkillButton);
		HandleButtonIconsForSkill(Skill.Whirlwind, enemyActionType, WhirlwindSkillButton);
		HandleButtonIconsForSkill(Skill.Hook, enemyActionType, HookSkillButton);
		HandleButtonIconsForSkill(Skill.Wrestle, enemyActionType, WrestleSkillButton);
		HandleButtonIconsForSkill(Skill.Shove, enemyActionType, ShoveSkillButton);
		HandleButtonIconsForSkill(Skill.LightningReflexes, enemyActionType, LightningReflexesSkillButton);
    }

	void HandleButtonIconsForSkill(Skill skill, SkillType enemyActionType, SkillButton skillButton)
	{
		Resource cost = skill.GetTotalCost(enemyActionType);
		int damage = skill.GetDamageAgainstEnemyAction(Skill.GetSkillForType(enemyActionType));
		skillButton.HandleCostAndDamage(cost, damage);
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

	public void OnMouseButtonEnterOnSkill(SkillType skillType)
	{
		if (skillType == SkillType.Wrestle)
		{
			CurrentEnemy.currentHex.SetAffectedBySkill(true);
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

    public void OnEmptyClick()
    {
        SkillsParent.SetActive(false);
        if (CurrentEnemy != null)
        {
            CurrentEnemy.currentHex.UnselectAsTarget();
			CurrentEnemy.currentHex.SetAffectedBySkill(false);
        }
        CurrentEnemy = null;

		if (CurrentTurnState == TurnState.PlayerAnswer)
		{
			SidestepSkillButton.gameObject.SetActive(!Player.instance.sidestepUsed);
			HandleButtonIconsForSkill(Skill.Sidestep, SkillType.None, SidestepSkillButton);
			Player.instance.currentHex.RevertAdjacentHighlights();
			isSidestepActive = false;
		}
    }

    public void RegisterPlayerAction(Skill reaction, int damage, Resource skillCost)
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
			Ray ray = new Ray(Player.instance.transform.position, CurrentEnemy.transform.position - Player.instance.transform.position);
			RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 2f, 1 << 8);
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].collider.CompareTag("Enemy"))
				{
					answeredEnemies.Add(hits[i].collider.GetComponent<Enemy>());
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
		isTutorialActive = true;
    }

	public void OnBackToGameClicked()
	{
		tutorialCanvas.SetActive(false);
		isTutorialActive = false;
		if (!IsGameOver && CurrentTurnState != TurnState.Clash)
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
