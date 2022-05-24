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
	public GameObject focusIconPrefab;
	public GameObject strengthIconPrefab;
	public GameObject stabilityIconPrefab;

    List<Image> FocusIcons;
    List<Image> StrengthIcons;
    List<Image> StabilityIcons;
	public Sprite FocusSprite;
	public Sprite StrengthSprite;
	public Sprite StabilitySprite;
	public Sprite InjurySprite;

	public GameObject hawkFocusButton;
	public GameObject bullStrengthButton;
	public GameObject turtleStabilityButton;
	public GameObject hawkFocusIndicator;
	public GameObject bullStrengthIndicator;
	public GameObject turtleStabilityIndicator;

	public GameObject armorIndicator;
	public GameObject armorInjuryIndicator;
	public Text armorText;

    public GameObject[] InjuryIcons;

    public Resource ResourceRecharge;
    public Resource CurrentResource;
    public Resource MaxResource;

	public List<Item> items;

	public Hex currentHex;

    SkillButton HeavyAttackButton;
    SkillButton SwiftAttackButton;
    SkillButton BlockButton;
    SkillButton CounterButton;
    SkillButton KillingBlowButton;
	SkillButton DeflectArrowButton;
	SkillButton SkewerButton;
	SkillButton BlockArrowButton;
	SkillButton WhirlwindButton;
	SkillButton SidestepButton;
	SkillButton HookButton;
	SkillButton WrestleButton;
	SkillButton ShoveButton;
	SkillButton HeartshotButton;
	SkillButton LightningReflexesButton;

    int CurrentInjury;
    int MaxInjury;
	int totalArmor = 0;
	bool injuryBlockedThisTurn = false;
	public bool sidestepUsed = false;
	public bool wrestleUsed = false;
	bool pendingInjuryFromSidestep = false;

	int hawkFocusRemaining = 0;
	int bullStrengthRemaining = 0;
	int turtleStabilityRemaining = 0;

	int powerupDuration = 3;

    void Awake()
    {
        instance = this;

        InitResources();

        CurrentResource = new Resource
        {
            Focus = MaxResource.Focus,
            Strength = MaxResource.Strength,
            Stability = MaxResource.Stability
        };

        CurrentInjury = 0;
        MaxInjury = InjuryIcons.Length;
    }

	void InitResources()
	{
		FocusIcons = new List<Image>();
		for (int i = 0; i < MaxResource.Focus; i++)
		{
			Image icon = Instantiate(focusIconPrefab, focusIconsParent.transform).GetComponent<Image>();
			FocusIcons.Add(icon);
		}

		StrengthIcons = new List<Image>();
		for (int i = 0; i < MaxResource.Strength; i++)
		{
			Image icon = Instantiate(strengthIconPrefab, strengthIconsParent.transform).GetComponent<Image>();
			StrengthIcons.Add(icon);
		}

		StabilityIcons = new List<Image>();
		for (int i = 0; i < MaxResource.Stability; i++)
		{
			Image icon = Instantiate(stabilityIconPrefab, stabilityIconsParent.transform).GetComponent<Image>();
			StabilityIcons.Add(icon);
		}
	}

    void Start()
    {
        HeavyAttackButton = GameController.instance.HeavyAttackSkillButton;
        SwiftAttackButton = GameController.instance.SwiftAttackSkillButton;
        BlockButton = GameController.instance.BlockSkillButton;
        CounterButton = GameController.instance.CounterSkillButton;
        KillingBlowButton = GameController.instance.KillingBlowSkillButton;
		DeflectArrowButton = GameController.instance.DeflectArrowSkillButton;
		SkewerButton = GameController.instance.SkewerSkillButton;
		BlockArrowButton = GameController.instance.BlockArrowSkillButton;
		WhirlwindButton = GameController.instance.WhirlwindSkillButton;
		SidestepButton = GameController.instance.SidestepSkillButton;
		HookButton = GameController.instance.HookSkillButton;
		WrestleButton = GameController.instance.WrestleSkillButton;
		ShoveButton = GameController.instance.ShoveSkillButton;
		HeartshotButton = GameController.instance.HeartshotSkillButton;
		LightningReflexesButton = GameController.instance.LightningReflexesSkillButton;
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
			Focus = FocusIcons.Count,
			Strength = StrengthIcons.Count,
			Stability = StabilityIcons.Count
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
		hawkFocusIndicator.SetActive(true);
	}

	public void OnBullStrengthClicked()
	{
		bullStrengthRemaining = powerupDuration;
		strengthIconsParent.SetActive(false);
		CurrentResource.Strength = int.MaxValue - 100;	// much brain
		bullStrengthButton.SetActive(false);
		bullStrengthIndicator.SetActive(true);
	}

	public void OnTurtleStabilityClicked()
	{
		turtleStabilityRemaining = powerupDuration;
		stabilityIconsParent.SetActive(false);
		CurrentResource.Stability = int.MaxValue - 100;	// wow
		turtleStabilityButton.SetActive(false);
		turtleStabilityIndicator.SetActive(true);
	}

    public void RechargeResources(Resource styleModifier)
    {
        // sad shit
        //CurrentResource += ResourceRecharge;
		hawkFocusRemaining = Mathf.Max(hawkFocusRemaining - 1, 0);
		bullStrengthRemaining = Mathf.Max(bullStrengthRemaining - 1, 0);
		turtleStabilityRemaining = Mathf.Max(turtleStabilityRemaining - 1, 0);

		if (CurrentResource.Focus == 0 && CurrentResource.Strength == 0 && CurrentResource.Stability == 0)
		{
			styleModifier += 1;
		}

		if (hawkFocusRemaining == 0)
		{
			CurrentResource.Focus = Mathf.Clamp(CurrentResource.Focus + ResourceRecharge.Focus + styleModifier.Focus, 0, MaxResource.Focus);
			hawkFocusIndicator.SetActive(false);
		}
		if (bullStrengthRemaining == 0)
		{
			CurrentResource.Strength = Mathf.Clamp(CurrentResource.Strength + ResourceRecharge.Strength + styleModifier.Strength, 0, MaxResource.Strength);
			bullStrengthIndicator.SetActive(false);
		}
		if (turtleStabilityRemaining == 0)
		{
			CurrentResource.Stability = Mathf.Clamp(CurrentResource.Stability + ResourceRecharge.Stability + styleModifier.Stability, 0, MaxResource.Stability);
			turtleStabilityIndicator.SetActive(false);
		}
        
        HandleResourceIcons();
		injuryBlockedThisTurn = false;
		armorInjuryIndicator.SetActive(false);
		if (totalArmor == 0)
		{
			armorIndicator.SetActive(false);
		}
		sidestepUsed = false;
		wrestleUsed = false;
		pendingInjuryFromSidestep = false;
    }

    public void GetInjury()
    {
		if (totalArmor > 0 && !injuryBlockedThisTurn)
		{
			injuryBlockedThisTurn = true;
			totalArmor--;
			armorInjuryIndicator.SetActive(true);
			UpdateArmorText();
		}
		else if (CurrentInjury == MaxInjury)
        {
            GameController.instance.HandlePlayerDeath();
        }
        else
        {
            InjuryIcons[CurrentInjury].SetActive(true);
            CurrentInjury++;
            MaxResource = (MaxResource / 2) + 1;
			CurrentResource.ClampToReference(MaxResource);
			if (hawkFocusRemaining > 0)
			{
				CurrentResource.Focus = int.MaxValue - 100;
			}
			if (bullStrengthRemaining > 0)
			{
				CurrentResource.Strength = int.MaxValue - 100;
			}
			if (turtleStabilityRemaining > 0)
			{
				CurrentResource.Stability = int.MaxValue - 100;
			}
            HandleResourceIcons();
        }
    }

    public void OnSkillClicked(Resource skillCost, Skill skill, int damage)
    {
		Resource previouslySpent = GameController.instance.GetResourceSpentOnCurrentEnemy(skill);
		Resource unspentResource = CurrentResource + previouslySpent;

        if (skillCost <= unspentResource)
        {
			if (skill == Skill.Sidestep)
			{
				currentHex.HighlightValidAdjacents();
				GameController.instance.isSidestepActive = true;
			}
			else if (skill == Skill.Wrestle)
			{
				CurrentResource = unspentResource - skillCost;
				HandleResourceIcons();
				GameController.instance.OnWrestle();
				wrestleUsed = true;
			}
			else
			{
				CurrentResource = unspentResource - skillCost;
				GameController.instance.RegisterPlayerAction(skill, damage, skillCost);
				HandleResourceIcons();
			}
        }
    }

	public void OnPlayerSidestep(Resource resourceGivenBack)
	{
		CurrentResource = CurrentResource + resourceGivenBack - SidestepButton.Cost;
		if (pendingInjuryFromSidestep)
		{
			GetInjury();
			pendingInjuryFromSidestep = false;
		}
		HandleResourceIcons();
	}

	public void MovePlayer(Hex newHex, bool isSidestep = false)
	{
		if (isSidestep && GameController.instance.IsAggressiveEnemyAdjacentToPlayer())
		{
			//pendingInjuryFromSidestep = true;
		}

		currentHex.RevertAdjacentHighlights();
		currentHex.isOccupiedByPlayer = false;
		transform.position = newHex.transform.position + Hex.posOffset;
		currentHex = newHex;
		newHex.isOccupiedByPlayer = true;

		sidestepUsed = isSidestep;
	}

	public void PickItem(Item newItem)
	{
		items.Add(newItem);
		if (newItem.itemType == ItemType.resourceBooster)
		{
			IncreaseMaxResource(newItem.resourceModifier);
			HandleResourceIcons();
		}
		else if (newItem.itemType == ItemType.unlocksSkill)
		{
			GameController.instance.OnSkillUnlocked(newItem.unlockedSkillType);
		}
		else if (newItem.itemType == ItemType.armor)
		{
			armorIndicator.SetActive(true);
			totalArmor += newItem.armorGiven;
			UpdateArmorText();
		}
		else if (newItem.itemType == ItemType.rechargeBooster)
		{
			IncreaseResourceRecharge(newItem.resourceModifier);
		}
		else if (newItem.itemType == ItemType.infiniteResourcePowerup)
		{
			if (newItem.resourceModifier.Focus > 0)
			{
				hawkFocusButton.SetActive(true);
			}
			else if (newItem.resourceModifier.Strength > 0)
			{
				bullStrengthButton.SetActive(true);
			}
			else if (newItem.resourceModifier.Stability > 0)
			{
				turtleStabilityButton.SetActive(true);
			}
		}
	}

	void UpdateArmorText()
	{
		armorText.text = totalArmor.ToString();
	}

	void IncreaseResourceRecharge(Resource resource)
	{
		ResourceRecharge += resource;
	}

	void IncreaseMaxResource(Resource resource)
	{
		MaxResource += resource;
		for (int i = 0; i < resource.Focus; i++)
		{
			Image icon = Instantiate(focusIconPrefab, focusIconsParent.transform).GetComponent<Image>();
			FocusIcons.Add(icon);
		}
		for (int i = 0; i < resource.Strength; i++)
		{
			Image icon = Instantiate(strengthIconPrefab, strengthIconsParent.transform).GetComponent<Image>();
			StrengthIcons.Add(icon);
		}
		for (int i = 0; i < resource.Stability; i++)
		{
			Image icon = Instantiate(stabilityIconPrefab, stabilityIconsParent.transform).GetComponent<Image>();
			StabilityIcons.Add(icon);
		}
		CurrentResource += resource;
	}

	void HandleIconsForResource(int powerupRemaining, GameObject parent, List<Image> icons, int current, int max, Sprite sprite)
	{
		if (powerupRemaining == 0)
		{
			parent.SetActive(true);
			for (int i = 0; i < icons.Count; i++)
			{
				if (i < current)
				{
					icons[i].color = ActiveResourceColor;
					icons[i].sprite = sprite;
				}
				else if (i < max)
				{
					icons[i].color = InactiveResourceColor;
					icons[i].sprite = sprite;
				}
				else
				{
					icons[i].color = InactiveResourceColor;
					icons[i].sprite = InjurySprite;
				}
			}
		}
	}

    void HandleResourceIcons()
    {
		HandleIconsForResource(hawkFocusRemaining, focusIconsParent, FocusIcons, CurrentResource.Focus, MaxResource.Focus, FocusSprite);
		HandleIconsForResource(bullStrengthRemaining, strengthIconsParent, StrengthIcons, CurrentResource.Strength, MaxResource.Strength, StrengthSprite);
		HandleIconsForResource(turtleStabilityRemaining, stabilityIconsParent, StabilityIcons, CurrentResource.Stability, MaxResource.Stability, StabilitySprite);
    }
}
