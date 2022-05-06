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
	public SkillButton DeflectArrowSkillButton;

    public Button TurnProgressButton;

    public GameObject DeathPanel;

    public GameObject BloodEffectPrefab;

    Dictionary<Enemy, Clash> Clashes = new Dictionary<Enemy, Clash>();
    List<Enemy> EnemiesMarkedForDeath = new List<Enemy>();

    Enemy CurrentEnemy;

    public TurnState CurrentTurnState;

    public bool IsGameOver;

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

        foreach (KeyValuePair<Enemy, Clash> clash in Clashes)
		{
			clash.Key.MoveTurn();
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

    IEnumerator ProcessCombat()
    {
        foreach (KeyValuePair<Enemy, Clash> clash in Clashes)
        {
			clash.Value.Action.HandleClash(clash.Key, clash.Value.Reaction);
			if (clash.Value.Reaction == null && clash.Key.IsDefensive())
			{
				continue;
			}
			HandleClashAnimations(clash.Key, clash.Value.Action, clash.Value.Reaction);

			yield return new WaitForSeconds(1.5f);

			clash.Key.transform.position = clash.Key.currentHex.transform.position + Hex.posOffset;
			Player.instance.transform.position = Player.instance.currentHex.transform.position + Hex.posOffset;
        }

        KillMarkedEnemies();
        Player.instance.RechargeResources();
        ResetClashes();
        ProgressTurn();
    }

	void HandleClashAnimations(Enemy enemy, Skill enemyAction, Skill playerReaction)
	{
		bool playerShouldFaceLeft = enemy.transform.position.x < Player.instance.transform.position.x;
		Player.instance.SetRendererFlip(playerShouldFaceLeft);
		enemy.SetRendererFlip(!playerShouldFaceLeft);

		if (playerReaction != null)
		{
			Player.instance.animator.Play(playerReaction.clip);
		}

		enemy.animator.Play(enemyAction.clip);


		Vector3 basePos = Player.instance.transform.position;
		if (enemyAction != Skill.ShootArrow)
		{
			Vector3 offset = playerShouldFaceLeft ? new Vector3(-0.3f, 0f, 0f) : new Vector3(0.3f, 0f, 0f);
			enemy.transform.position = basePos + offset;

			Player.instance.transform.position -= offset;
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
            Clashes.Remove(temp);
            EnemiesMarkedForDeath.Remove(temp);
            Instantiate(BloodEffectPrefab, temp.transform.position, Quaternion.identity);
			temp.currentHex.isOccupiedByEnemy = false;
            Destroy(temp.gameObject);
        }

        if (Clashes.Count == 0)
        {
            WaveManager.instance.SendNewWave();
			Player.instance.ResetInjuries();
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
                CurrentEnemy.currentHex.UnselectAsTarget();
            }

            CurrentEnemy = enemy;

            CurrentEnemy.currentHex.SelectAsTarget();
            
            SkillsParent.SetActive(true);

            HandleSkillButtonIcons();
			
			bool isEnemyShootingArrow = Clashes[CurrentEnemy].Action.Type == SkillType.ShootArrow;
			bool isEnemyDefensive = CurrentEnemy.IsDefensive();
			bool isEnemyVulnerable = CurrentEnemy.IsVulnerable;

			BlockSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyDefensive && !isEnemyVulnerable);
			CounterSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyDefensive && !isEnemyVulnerable);
			SwiftAttackSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyVulnerable);
			HeavyAttackSkillButton.gameObject.SetActive(!isEnemyShootingArrow && !isEnemyVulnerable);
			KillingBlowSkillButton.gameObject.SetActive(!isEnemyShootingArrow);
			DeflectArrowSkillButton.gameObject.SetActive(isEnemyShootingArrow && !isEnemyVulnerable);
        }
    }

    public Resource GetResourceSpentOnCurrentEnemy()
    {
        Clash currentClash = Clashes[CurrentEnemy];
        if (currentClash.Reaction != null)
        {
            return currentClash.Reaction.GetTotalCost(currentClash.Action.Type);
        }
        else
        {
            return new Resource();
        }
    }

    void HandleSkillButtonIcons()
    {
		SkillType enemyActionType = Clashes[CurrentEnemy].Action.Type;

        HeavyAttackSkillButton.HandleCostAndDamage(Skill.HeavyAttack.GetTotalCost(enemyActionType),
									Skill.HeavyAttack.GetDamageAgainstEnemyAction(enemyActionType));

        SwiftAttackSkillButton.HandleCostAndDamage(Skill.SwiftAttack.GetTotalCost(enemyActionType),
									Skill.SwiftAttack.GetDamageAgainstEnemyAction(enemyActionType));

        BlockSkillButton.HandleCostAndDamage(Skill.Block.GetTotalCost(enemyActionType),
									Skill.Block.GetDamageAgainstEnemyAction(enemyActionType));

        CounterSkillButton.HandleCostAndDamage(Skill.Counter.GetTotalCost(enemyActionType),
									Skill.Counter.GetDamageAgainstEnemyAction(enemyActionType));

        KillingBlowSkillButton.HandleCostAndDamage(Skill.KillingBlow.GetTotalCost(enemyActionType),
									Skill.KillingBlow.GetDamageAgainstEnemyAction(enemyActionType));

		DeflectArrowSkillButton.HandleCostAndDamage(Skill.DeflectArrow.GetTotalCost(enemyActionType),
									Skill.DeflectArrow.GetDamageAgainstEnemyAction(enemyActionType));
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
            CurrentEnemy.currentHex.UnselectAsTarget();
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
