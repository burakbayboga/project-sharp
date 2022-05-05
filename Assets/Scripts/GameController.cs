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
    
    public SkillButton SwiftAttackSkillButton;
    public SkillButton HeavyAttackSkillButton;
    public SkillButton BlockSkillButton;
    public SkillButton CounterSkillButton;
    public SkillButton KillingBlowSkillButton;

    public Button TurnProgressButton;

    public GameObject DeathPanel;

    public GameObject BloodEffectPrefab;

    Button BlockButton;
    Button CounterButton;

    Dictionary<Enemy, Clash> Clashes = new Dictionary<Enemy, Clash>();
    List<Enemy> EnemiesMarkedForDeath = new List<Enemy>();

    Enemy CurrentEnemy;

    TurnState CurrentTurnState;

    public bool IsGameOver;

    void Awake()
    {
        instance = this;

        BlockButton = BlockSkillButton.GetComponent<Button>();
        CounterButton = CounterSkillButton.GetComponent<Button>();
        IsGameOver = false;
    }

    void Start()
    {
        CurrentTurnState = TurnState.NewTurn;
        TurnStateText.text = CurrentTurnState.ToString();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            Clashes.Add(enemies[i].GetComponent<Enemy>(), new Clash());
        }

        Skill.InitSkills();
    }

    public void RegisterEnemies(Enemy[] newEnemies)
    {
        for (int i = 0; i < newEnemies.Length; i++)
        {
            Clashes.Add(newEnemies[i], new Clash());
        }
    }

	void MakeEnemiesMove()
	{
		// TODO: should enemies even move?
		CurrentTurnState = TurnState.EnemyMovement;
        TurnStateText.text = CurrentTurnState.ToString();
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
				CycleTurn();
				break;
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
	}

    void ProcessCombat()
    {
        foreach (KeyValuePair<Enemy, Clash> clash in Clashes)
        {
            ProcessClash(clash.Key, clash.Value);
        }
    }

    public void MarkEnemyForDeath(Enemy enemy)
    {
        EnemiesMarkedForDeath.Add(enemy);
    }

    void ProcessClash(Enemy enemy, Clash clash)
    {
        if (enemy.IsVulnerable && clash.Reaction == Skill.KillingBlow)
        {
            MarkEnemyForDeath(enemy);
        }
        else
        {
			if (clash.Action != null)
			{
				if (clash.Reaction != null)
				{
					clash.Action.HandleClash(enemy, clash.Reaction.Type);
				}
				else
				{
					clash.Action.HandleClash(enemy, SkillType.None);
				}
			}
        }
    }

    public void OnPlayAgainClicked()
    {
        SceneManager.LoadScene("game");
    }

    void EndTurn()
    {
		CurrentTurnState = TurnState.Clash;
        TurnStateText.text = CurrentTurnState.ToString();
        ProcessCombat();

        KillMarkedEnemies();

        Player.instance.RechargeResources();
        ResetClashes();

        ProgressTurn();
    }

    void KillMarkedEnemies()
    {
        while (EnemiesMarkedForDeath.Count > 0)
        {
            Enemy temp = EnemiesMarkedForDeath[0];
            Clashes.Remove(temp);
            EnemiesMarkedForDeath.Remove(temp);
            Instantiate(BloodEffectPrefab, temp.transform.position, Quaternion.identity);
            Destroy(temp.gameObject);
        }

        if (Clashes.Count == 0)
        {
            WaveManager.instance.SendNewWave();
        }
    }

    void ResetClashes()
    {
        foreach (KeyValuePair<Enemy, Clash> clash in Clashes)
        {
            clash.Key.ResetIcons();
            clash.Value.Reaction = null;
            clash.Value.Action = null;
        }
    }

    void EnterEnemyActionTurn()
    {
        CurrentTurnState = TurnState.EnemyAction;
        TurnStateText.text = CurrentTurnState.ToString();

        foreach (KeyValuePair<Enemy, Clash> clash in Clashes)
        {
            clash.Key.RegisterAction();
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
                CurrentEnemy.Highlight.SetActive(false);
            }

            CurrentEnemy = enemy;

            CurrentEnemy.Highlight.SetActive(true);
            
            SkillsParent.SetActive(true);

            HandleSkillCosts();

            if (IsCurrentEnemyDefensive())
            {
                BlockButton.interactable = false;
                CounterButton.interactable = false;
            }
            else
            {
                BlockButton.interactable = true;
                CounterButton.interactable = true;
            }
        }
    }

    bool IsCurrentEnemyDefensive()
    {
        SkillType actionType = Clashes[CurrentEnemy].Action.Type;
        return actionType == SkillType.Block || actionType == SkillType.Counter;
    }

    public Resource GetResourceSpentOnCurrentEnemy()
    {
        Clash currentClash = Clashes[CurrentEnemy];
        if (currentClash.Reaction != null)
        {
            return currentClash.Reaction.GetTotalCost(currentClash.Action.Type, out int damage);
        }
        else
        {
            return new Resource();
        }
    }

    void HandleSkillCosts()
    {
        int damage;
        HeavyAttackSkillButton.HandleCostAndDamage(Skill.HeavyAttack.GetTotalCost(Clashes[CurrentEnemy].Action.Type, out damage), damage);
        SwiftAttackSkillButton.HandleCostAndDamage(Skill.SwiftAttack.GetTotalCost(Clashes[CurrentEnemy].Action.Type, out damage), damage);
        BlockSkillButton.HandleCostAndDamage(Skill.Block.GetTotalCost(Clashes[CurrentEnemy].Action.Type, out damage), damage);
        CounterSkillButton.HandleCostAndDamage(Skill.Counter.GetTotalCost(Clashes[CurrentEnemy].Action.Type, out damage), damage);
        KillingBlowSkillButton.HandleCostAndDamage(Skill.KillingBlow.GetTotalCost(Clashes[CurrentEnemy].Action.Type, out damage), damage);
    }

    public void OnPlayerClicked()
    {

    }



    public void OnProgressTurnClicked()
    {
        ProgressTurn();
        SkillsParent.SetActive(false);
    }

    public void OnButtonMouseDown()
    {
        IngameInput.instance.IsIngameInputActive = false;
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
            CurrentEnemy.Highlight.SetActive(false);
        }
        CurrentEnemy = null;
    }

    public void RegisterEnemyAction(Enemy enemy, Skill action)
    {
        Clashes[enemy].Action = action;
    }

    public void RegisterPlayerAction(Skill reaction, int damage)
    {
        Clashes[CurrentEnemy].Reaction = reaction;
        if (reaction == null)
        {
            CurrentEnemy.SetReactionImageAndWeaknessCue(SkillType.None, damage);
        }
        else
        {
            CurrentEnemy.SetReactionImageAndWeaknessCue(reaction.Type, damage);
        }
    }

    public void OnHowToPlayClicked()
    {
        SceneManager.LoadScene("howToPlay");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}


public class Clash
{
    public Skill Action;
    public Skill Reaction;
}

public enum TurnState
{
	NewTurn,		// turn start, player resolve
	EnemyMovement,	// enemies move, auto resolve
	PlayerMovement,	// player moves, player resolve
	EnemyAction,	// enemy actions appear, auto resolve
	PlayerAnswer,	// player answers, player resolve
	Clash			// handle clash, auto resolve
}
