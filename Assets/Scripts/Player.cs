using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player instance;

    public Color InactiveResourceColor;
    public Color ActiveResourceColor;

    public Image[] FocusIcons;
    public Image[] StrengthIcons;
    public Image[] StabilityIcons;

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

    int CurrentInjury;
    int MaxInjury;

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
            Focus = 3,
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
    }

    public void RechargeResources()
    {
        // sad shit
        //CurrentResource += ResourceRecharge;
        CurrentResource.Focus = Mathf.Clamp(CurrentResource.Focus + 3, 0, MaxResource.Focus);
        CurrentResource.Strength = Mathf.Clamp(CurrentResource.Strength + 2, 0, MaxResource.Strength);
        CurrentResource.Stability = Mathf.Clamp(CurrentResource.Stability + 3, 0, MaxResource.Stability);
        
        HandleResourceIcons();


		currentHex.HighlightValidAdjacents();
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
            MaxResource /= 2;
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

            if (i < CurrentResource.Strength)
            {
                StrengthIcons[i].color = ActiveResourceColor;
            }
            else
            {
                StrengthIcons[i].color = InactiveResourceColor;
            }

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
