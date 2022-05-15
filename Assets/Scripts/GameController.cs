using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public static GameController instance;

    public Text TurnStateText;
    public GameObject SkillsParent;

	public LootPanel lootPanel;
    
    public SkillButton SwiftAttackSkillButton;
    public SkillButton HeavyAttackSkillButton;
    public SkillButton BlockSkillButton;
    public SkillButton CounterSkillButton;
    public SkillButton KillingBlowSkillButton;
	public SkillButton DeflectArrowSkillButton;
	public SkillButton SkewerSkillButton;
	public SkillButton BlockArrowSkillButton;
	public SkillButton WhirlwindSkillButton;

	// TODO: skill unlock system
	bool isSkewerUnlocked;
	bool isBlockArrowUnlocked;
	bool isWhirlwindUnlocked;

    public Button TurnProgressButton;
	public Text unansweredEnemyText;
	int unansweredEnemyCount;

    public GameObject DeathPanel;

    public GameObject BloodEffectPrefab;

    List<Clash> Clashes = new List<Clash>();
	List<Enemy> Enemies = new List<Enemy>();
    List<Enemy> EnemiesMarkedForDeath = new List<Enemy>();

    Enemy CurrentEnemy;

    public TurnState CurrentTurnState;

    public bool IsGameOver;

	bool pendingLootTurn;

    void Awake()
    {
        instance = this;
        
        IsGameOver = false;
    }

    void Start()
    {
        CurrentTurnState = TurnState.NewTurn;
        TurnStateText.text = CurrentTurnState.ToString();

		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
			Enemies.Add(enemies[i].GetComponent<Enemy>());
        }
        Skill.InitSkills();
    }

	public void RegisterNewEnemies(List<Enemy> newEnemies)
	{
		Enemies.AddRange(newEnemies);
		UpdateUnansweredEnemyText();
	}

	void UpdateUnansweredEnemyText()
	{
		unansweredEnemyText.text = unansweredEnemyCount.ToString() + " Enemies\nNot Answered";
	}

	void MakeEnemiesMove()
	{
		// TODO: should enemies even move?
		CurrentTurnState = TurnState.EnemyMovement;
        TurnStateText.text = CurrentTurnState.ToString();

		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemies[i].MoveTurn();
		}
        
		ProgressTurn();
	}

	void EnterPlayerMoveTurn()
	{
		CurrentTurnState = TurnState.PlayerMovement;
        TurnStateText.text = CurrentTurnState.ToString();
		Player.instance.currentHex.HighlightValidAdjacents();
	}

	public void OnPlayerMove()
	{
		ProgressTurn();
	}

    void ProgressTurn()
    {
        switch (CurrentTurnState)
        {
			case TurnState.NewTurn:
				MakeEnemiesMove();
				break;
			case TurnState.EnemyMovement:
				EnterPlayerMoveTurn();
				break;
			case TurnState.PlayerMovement:
				Player.instance.currentHex.RevertAdjacentHighlights();
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
		if (lootPanel.PoolHasItem())
		{
			lootPanel.gameObject.SetActive(true);
			lootPanel.Fill();
			TurnProgressButton.interactable = false;
		}
		else
		{
			ProgressTurn();
		}
	}

	void CycleTurn()
	{
		CurrentTurnState = TurnState.NewTurn;
        TurnStateText.text = CurrentTurnState.ToString();
	}

	void EnterPlayerAnswerTurn()
	{
		CurrentTurnState = TurnState.PlayerAnswer;
        TurnStateText.text = CurrentTurnState.ToString();

		unansweredEnemyCount = 0;
		for (int i = 0; i < Enemies.Count; i++)
		{
			if (Enemies[i].CurrentAction != null)
			{
				unansweredEnemyCount++;
			}
		}
		UpdateUnansweredEnemyText();
		unansweredEnemyText.gameObject.SetActive(true);
	}

    IEnumerator ProcessCombat()
    {
		for (int i = 0; i < Enemies.Count; i++)
		{
			Enemy enemy = Enemies[i];
			if (enemy.CurrentClash == null)
			{
				Clash newClash = new Clash(new List<Enemy>{ enemy }, null, new Resource());
				enemy.CurrentClash = newClash;
				Clashes.Add(newClash);
			}
		}

		for (int i = 0; i < Clashes.Count; i++)
		{
			Clash clash = Clashes[i];
			for (int j = 0; j < clash.enemies.Count; j++)
			{
				Enemy enemy = clash.enemies[j];
				if (enemy.CurrentAction == null)
				{
					continue;
				}
				enemy.CurrentAction.HandleClash(enemy, clash.playerAnswer);
			}
			yield return StartCoroutine(HandleClashAnimations(clash));
		}

        KillMarkedEnemies();
        Player.instance.RechargeResources();
        ResetClashes();
		TurnProgressButton.interactable = true;
        ProgressTurn();
    }

	IEnumerator HandleClashAnimations(Clash clash)
	{
		if (clash.playerAnswer == Skill.Skewer
				|| clash.playerAnswer == Skill.Whirlwind)
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

			if (enemyAction == null || (enemy.IsDefensive() && playerReaction == null))
			{
				yield break;
			}

			bool playerShouldFaceLeft = enemy.transform.position.x < Player.instance.transform.position.x;
			Player.instance.SetRendererFlip(playerShouldFaceLeft);
			enemy.SetRendererFlip(!playerShouldFaceLeft);

			if (playerReaction != null)
			{
				Player.instance.animator.Play(playerReaction.clip);
			}
			enemy.animator.Play(enemyAction.clip);


			Vector3 basePos = enemy.IsDefensive() ? enemy.transform.position : Player.instance.transform.position;
			if (enemyAction != Skill.ShootArrow)
			{
				Vector3 offset = playerShouldFaceLeft ? new Vector3(-0.3f, 0f, 0f) : new Vector3(0.3f, 0f, 0f);
				enemy.transform.position = basePos + offset;

				Player.instance.transform.position = basePos - offset;
			}


			yield return new WaitForSeconds(1.5f);

			enemy.transform.position = enemy.currentHex.transform.position + Hex.posOffset;
			Player.instance.transform.position = Player.instance.currentHex.transform.position + Hex.posOffset;
		}
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
		unansweredEnemyText.gameObject.SetActive(false);
		TurnProgressButton.interactable = false;
		if (CurrentEnemy != null)
		{
			CurrentEnemy.currentHex.UnselectAsTarget();
		}
		CurrentTurnState = TurnState.Clash;
        TurnStateText.text = CurrentTurnState.ToString();
        StartCoroutine(ProcessCombat());
    }

    void KillMarkedEnemies()
    {
        while (EnemiesMarkedForDeath.Count > 0)
        {
            Enemy temp = EnemiesMarkedForDeath[0];
            Enemies.Remove(temp);
            EnemiesMarkedForDeath.Remove(temp);
            Instantiate(BloodEffectPrefab, temp.transform.position, Quaternion.identity);
			temp.currentHex.isOccupiedByEnemy = false;
            Destroy(temp.gameObject);
        }

        if (Enemies.Count == 0)
        {
            WaveManager.instance.SendNewWave();
			Player.instance.ResetInjuries();
			pendingLootTurn = true;
        }
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
        TurnStateText.text = CurrentTurnState.ToString();

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
            if (CurrentEnemy != null)
            {
                CurrentEnemy.currentHex.UnselectAsTarget();
            }

            CurrentEnemy = enemy;

            CurrentEnemy.currentHex.SelectAsTarget();
            
            SkillsParent.SetActive(true);

            HandleSkillButtonIcons();
			
			bool isEnemyShootingArrow = CurrentEnemy.CurrentAction.Type == SkillType.ShootArrow;
			bool isEnemyDefensive = CurrentEnemy.IsDefensive();
			bool isEnemyVulnerable = CurrentEnemy.IsVulnerable;
			bool isAdjacentToEnemy = CurrentEnemy.currentHex.IsAdjacentToPlayer();

			BlockSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyDefensive && isAdjacentToEnemy);
			CounterSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyDefensive && isAdjacentToEnemy);
			SwiftAttackSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyVulnerable && isAdjacentToEnemy);
			HeavyAttackSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyVulnerable && isAdjacentToEnemy);
			SkewerSkillButton.gameObject.SetActive(isSkewerUnlocked && !isEnemyShootingArrow && !isEnemyVulnerable && isAdjacentToEnemy);
			KillingBlowSkillButton.gameObject.SetActive(isAdjacentToEnemy);
			DeflectArrowSkillButton.gameObject.SetActive(isEnemyShootingArrow);
			BlockArrowSkillButton.gameObject.SetActive(isBlockArrowUnlocked && isEnemyShootingArrow);
			WhirlwindSkillButton.gameObject.SetActive(isWhirlwindUnlocked && isAdjacentToEnemy);
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
		SkillType enemyActionType = CurrentEnemy.CurrentAction.Type;

		HandleButtonIconsForSkill(Skill.HeavyAttack, enemyActionType, HeavyAttackSkillButton);
		HandleButtonIconsForSkill(Skill.SwiftAttack, enemyActionType, SwiftAttackSkillButton);
		HandleButtonIconsForSkill(Skill.Block, enemyActionType, BlockSkillButton);
		HandleButtonIconsForSkill(Skill.Counter, enemyActionType, CounterSkillButton);
		HandleButtonIconsForSkill(Skill.KillingBlow, enemyActionType, KillingBlowSkillButton);
		HandleButtonIconsForSkill(Skill.DeflectArrow, enemyActionType, DeflectArrowSkillButton);
		HandleButtonIconsForSkill(Skill.Skewer, enemyActionType, SkewerSkillButton);
		HandleButtonIconsForSkill(Skill.BlockArrow, enemyActionType, BlockArrowSkillButton);
		HandleButtonIconsForSkill(Skill.Whirlwind, enemyActionType, WhirlwindSkillButton);
    }

	void HandleButtonIconsForSkill(Skill skill, SkillType enemyActionType, SkillButton skillButton)
	{
		Resource cost = skill.GetTotalCost(enemyActionType);
		int damage = skill.GetDamageAgainstEnemyAction(enemyActionType);
		skillButton.HandleCostAndDamage(cost, damage);
	}

    public void OnPlayerClicked()
    {

    }

    public void OnProgressTurnClicked()
    {
        ProgressTurn();
        SkillsParent.SetActive(false);
    }

    public void OnMouseButtonDownOnSkill()
    {
        IngameInput.instance.IsIngameInputActive = false;
    }

	public void OnMouseButtonEnterOnSkill(SkillType skillType)
	{
		List<Enemy> enemies = GetAnsweredEnemiesBySkill(Skill.GetSkillForType(skillType));
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].currentHex.SetAffectedBySkill(true);
		}
	}

	public void OnMouseButtonExitOnSkill(SkillType skillType)
	{
		List<Enemy> enemies = GetAnsweredEnemiesBySkill(Skill.GetSkillForType(skillType));
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].currentHex.SetAffectedBySkill(false);
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
        }
        CurrentEnemy = null;
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

			unansweredEnemyCount -= enemies.Count;
			UpdateUnansweredEnemyText();
		}
    }

	void EraseClash(Clash clash)
	{
		unansweredEnemyCount += clash.enemies.Count;
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
		else
		{
			answeredEnemies.Add(CurrentEnemy);
		}

		return answeredEnemies;
	}

    public void OnHowToPlayClicked()
    {
        SceneManager.LoadScene("howToPlay");
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
			default:
				break;
		}
	}

	public void OnItemClicked(Item item)
	{
		TurnProgressButton.interactable = true;
		lootPanel.OnItemPicked(item);
		lootPanel.gameObject.SetActive(false);
		ProgressTurn();
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
	NewTurn,		// turn start, player resolve
	EnemyMovement,	// enemies move, auto resolve
	PlayerMovement,	// player moves, player resolve
	EnemyAction,	// enemy actions appear, auto resolve
	PlayerAnswer,	// player answers, player resolve
	Clash,			// handle clash, auto resolve
	Loot			// new items. resolved when item is picked. this state is only reached after wave is cleared
}
