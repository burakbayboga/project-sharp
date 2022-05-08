using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player instance;

	public Animator animator;
	public SpriteRenderer rend;

    public Color InactiveResourceColor;
    public Color ActiveResourceColor;

	public GameObject focusIconsParent;
	public GameObject strengthIconsParent;
	public GameObject stabilityIconsParent;
    public Image[] FocusIcons;
    public Image[] StrengthIcons;
    public Image[] StabilityIcons;

	public GameObject hawkFocusButton;
	public GameObject bullStrengthButton;
	public GameObject turtleStabilityButton;

    public GameObject[] InjuryIcons;

    public Resource ResourceRecharge;
    public Resource CurrentResource;
    public Resource MaxResource;

	public Hex currentHex;

    SkillButton HeavyAttackButton;
    SkillButton SwiftAttackButton;
    SkillButton BlockButton;
    SkillButton CounterButton;
    SkillButton KillingBlowButton;
	SkillButton DeflectArrowButton;

    int CurrentInjury;
    int MaxInjury;

	int hawkFocusRemaining = 0;
	int bullStrengthRemaining = 0;
	int turtleStabilityRemaining = 0;

	int powerupDuration = 3;

    void Awake()
    {
        instance = this;

        MaxResource = new Resource
        {
            Focus = FocusIcons.Length,
            Strength = StrengthIcons.Length,
            Stability = StabilityIcons.Length
        };

        CurrentResource = new Resource
        {
            Focus = MaxResource.Focus,
            Strength = MaxResource.Strength,
            Stability = MaxResource.Stability
        };

        ResourceRecharge = new Resource
        {
            Focus = 4,
            Strength = 2,
            Stability = 3
        };

        CurrentInjury = 0;
        MaxInjury = InjuryIcons.Length;
    }

    void Start()
    {
        HeavyAttackButton = GameController.instance.HeavyAttackSkillButton;
        SwiftAttackButton = GameController.instance.SwiftAttackSkillButton;
        BlockButton = GameController.instance.BlockSkillButton;
        CounterButton = GameController.instance.CounterSkillButton;
        KillingBlowButton = GameController.instance.KillingBlowSkillButton;
		DeflectArrowButton = GameController.instance.DeflectArrowSkillButton;
    }

	public void SetRendererFlip(bool flip)
	{
		rend.flipX = flip;
	}

	public void ResetInjuries()
	{
		CurrentInjury = 0;
		MaxResource = new Resource
		{
			Focus = FocusIcons.Length,
			Strength = StrengthIcons.Length,
			Stability = StabilityIcons.Length
		};

		for (int i = 0; i < InjuryIcons.Length; i++)
		{
			InjuryIcons[i].SetActive(false);
		}
	}

	public void OnHawkFocusClicked()
	{
		hawkFocusRemaining = powerupDuration;
		focusIconsParent.SetActive(false);
		CurrentResource.Focus = int.MaxValue - 100;	// such workaround
		hawkFocusButton.SetActive(false);
	}

	public void OnBullStrengthClicked()
	{
		bullStrengthRemaining = powerupDuration;
		strengthIconsParent.SetActive(false);
		CurrentResource.Strength = int.MaxValue - 100;	// much brain
		bullStrengthButton.SetActive(false);
	}

	public void OnTurtleStabilityClicked()
	{
		turtleStabilityRemaining = powerupDuration;
		stabilityIconsParent.SetActive(false);
		CurrentResource.Stability = int.MaxValue - 100;	// wow
		turtleStabilityButton.SetActive(false);
	}

    public void RechargeResources()
    {
        // sad shit
        //CurrentResource += ResourceRecharge;
		hawkFocusRemaining = Mathf.Max(hawkFocusRemaining - 1, 0);
		bullStrengthRemaining = Mathf.Max(bullStrengthRemaining - 1, 0);
		turtleStabilityRemaining = Mathf.Max(turtleStabilityRemaining - 1, 0);


		if (hawkFocusRemaining == 0)
		{
			CurrentResource.Focus = Mathf.Clamp(CurrentResource.Focus + ResourceRecharge.Focus, 0, MaxResource.Focus);
		}
		if (bullStrengthRemaining == 0)
		{
			CurrentResource.Strength = Mathf.Clamp(CurrentResource.Strength + ResourceRecharge.Strength, 0, MaxResource.Strength);
		}
		if (turtleStabilityRemaining == 0)
		{
			CurrentResource.Stability = Mathf.Clamp(CurrentResource.Stability + ResourceRecharge.Stability, 0, MaxResource.Stability);
		}
        
        HandleResourceIcons();
    }

    public void GetInjury()
    {
        if (CurrentInjury == MaxInjury)
        {
            GameController.instance.HandlePlayerDeath();
        }
        else
        {
            InjuryIcons[CurrentInjury].SetActive(true);
            CurrentInjury++;
            MaxResource = (MaxResource / 2) + 1;
            CurrentResource += 0;   // blood for the blood god
            HandleResourceIcons();
        }
    }

    public void OnSkillClicked(int skillTypeValue)
    {
        SkillType skillType = (SkillType)skillTypeValue;

        Resource skillCost;
        Skill skill;
        int damage;

        switch (skillType)
        {
            case SkillType.HeavyAttack:
                skillCost = HeavyAttackButton.Cost;
                skill = Skill.HeavyAttack;
                damage = HeavyAttackButton.Damage;
                break;
            case SkillType.SwiftAttack:
                skillCost = SwiftAttackButton.Cost;
                skill = Skill.SwiftAttack;
                damage = SwiftAttackButton.Damage;
                break;
            case SkillType.Block:
                skillCost = BlockButton.Cost;
                skill = Skill.Block;
                damage = BlockButton.Damage;
                break;
            case SkillType.Counter:
                skillCost = CounterButton.Cost;
                skill = Skill.Counter;
                damage = CounterButton.Damage;
                break;
            case SkillType.KillingBlow:
                skillCost = KillingBlowButton.Cost;
                skill = Skill.KillingBlow;
                damage = KillingBlowButton.Damage;
                break;
			case SkillType.DeflectArrow:
				skillCost = DeflectArrowButton.Cost;
				skill = Skill.DeflectArrow;
				damage = DeflectArrowButton.Damage;
				break;
            case SkillType.None:
            default:
                skillCost = new Resource();
                skill = null;
                damage = 0;
                break;
        }

        Resource unspentResource = CurrentResource + GameController.instance.GetResourceSpentOnCurrentEnemy();

        if (skillCost <= unspentResource)
        {
            CurrentResource = unspentResource - skillCost;
            GameController.instance.RegisterPlayerAction(skill, damage);
            HandleResourceIcons();
        }
    }

    void HandleResourceIcons()
    {
		if (hawkFocusRemaining == 0)
		{
			focusIconsParent.SetActive(true);
			for (int i = 0; i < FocusIcons.Length; i++)
			{
				if (i < CurrentResource.Focus)
				{
					FocusIcons[i].color = ActiveResourceColor;
				}
				else
				{
					FocusIcons[i].color = InactiveResourceColor;
				}
			}
		}

		if (bullStrengthRemaining == 0)
		{
			strengthIconsParent.SetActive(true);
			for (int i = 0; i < StrengthIcons.Length; i++)
			{
				if (i < CurrentResource.Strength)
				{
					StrengthIcons[i].color = ActiveResourceColor;
				}
				else
				{
					StrengthIcons[i].color = InactiveResourceColor;
				}
			}
		}

		if (turtleStabilityRemaining == 0)
		{
			stabilityIconsParent.SetActive(true);
			for (int i = 0; i < StabilityIcons.Length; i++)
			{
				if (i < CurrentResource.Stability)
				{
					StabilityIcons[i].color = ActiveResourceColor;
				}
				else
				{
					StabilityIcons[i].color = InactiveResourceColor;
				}		
			}
		}
    }
}
