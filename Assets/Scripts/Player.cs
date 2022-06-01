using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player instance;

	public Animator animator;
	public SpriteRenderer rend;
	public Animator[] resourceAnimators;
	int wobbleAnim = Animator.StringToHash("resource wobble");

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

    int CurrentInjury;
    int MaxInjury;
	int totalArmor = 0;
	bool injuryBlockedThisTurn = false;
	public bool sidestepUsed = false;
	public bool wrestleUsed = false;
	public bool chargeUsed = false;
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

    public void RechargeResources(Resource styleModifier, int rechargeMultiplier)
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

		Resource turnRecharge = ResourceRecharge * rechargeMultiplier;

		if (hawkFocusRemaining == 0)
		{
			CurrentResource.Focus = Mathf.Clamp(CurrentResource.Focus + turnRecharge.Focus + styleModifier.Focus, 0, MaxResource.Focus);
			hawkFocusIndicator.SetActive(false);
		}
		if (bullStrengthRemaining == 0)
		{
			CurrentResource.Strength = Mathf.Clamp(CurrentResource.Strength + turnRecharge.Strength + styleModifier.Strength, 0, MaxResource.Strength);
			bullStrengthIndicator.SetActive(false);
		}
		if (turtleStabilityRemaining == 0)
		{
			CurrentResource.Stability = Mathf.Clamp(CurrentResource.Stability + turnRecharge.Stability + styleModifier.Stability, 0, MaxResource.Stability);
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
		chargeUsed = false;
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
		if (GameController.instance.isTutorialPanelActive)
		{
			return;
		}
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
			else if (skill == Skill.Charge)
			{
				CurrentResource = unspentResource - skillCost;
				HandleResourceIcons();
				GameController.instance.OnCharge();
				chargeUsed = true;
			}
			else
			{
				CurrentResource = unspentResource - skillCost;
				GameController.instance.RegisterPlayerAction(skill, damage, skillCost);
				HandleResourceIcons();
			}
        }
		else
		{
			StartCoroutine(WobbleResources(skillCost, unspentResource));
		}
    }

	IEnumerator WobbleResources(Resource skillCost, Resource unspentResource)
	{
		WaitForSeconds delay = new WaitForSeconds(0.05f);
		if (skillCost.Focus > unspentResource.Focus)
		{
			resourceAnimators[0].Play(wobbleAnim);
			yield return delay;
		}
		if (skillCost.Strength > unspentResource.Strength)
		{
			resourceAnimators[1].Play(wobbleAnim);
			yield return delay;
		}
		if (skillCost.Stability > unspentResource.Stability)
		{
			resourceAnimators[2].Play(wobbleAnim);
			yield return delay;
		}
	}

	public void OnPlayerSidestep(Resource resourceGivenBack)
	{
		CurrentResource = CurrentResource + resourceGivenBack - GameController.instance.SidestepSkillButton.Cost;
		if (pendingInjuryFromSidestep)
		{
			GetInjury();
			pendingInjuryFromSidestep = false;
		}
		HandleResourceIcons();
	}

	public void MovePlayer(Hex newHex, bool isSidestep = false, bool isWrestle = false)
	{
		if (isSidestep && GameController.instance.IsAggressiveEnemyAdjacentToPlayer())
		{
			//pendingInjuryFromSidestep = true;
		}

		currentHex.RevertAdjacentHighlights();
		currentHex.isOccupiedByPlayer = false;
		StartCoroutine(Run(newHex.transform.position + Hex.posOffset, isWrestle));
		currentHex = newHex;
		newHex.isOccupiedByPlayer = true;

		sidestepUsed = isSidestep;
	}

	IEnumerator Run(Vector3 target, bool isWrestle)
	{
		Vector3 start = transform.position;
		float startTime = Time.time;
		float t = 0f;
		SetRendererFlip(target.x < start.x);

		if (!isWrestle)
		{
			animator.Play("run");
		}
		while (t < 1f)
		{
			t = Mathf.Clamp01((Time.time - startTime) / 0.4f);
			transform.position = Vector3.Lerp(start, target, t);

			yield return null;
		}
		animator.Play("idle");
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

    public void HandleResourceIcons()
    {
		HandleIconsForResource(hawkFocusRemaining, focusIconsParent, FocusIcons, CurrentResource.Focus, MaxResource.Focus, FocusSprite);
		HandleIconsForResource(bullStrengthRemaining, strengthIconsParent, StrengthIcons, CurrentResource.Strength, MaxResource.Strength, StrengthSprite);
		HandleIconsForResource(turtleStabilityRemaining, stabilityIconsParent, StabilityIcons, CurrentResource.Stability, MaxResource.Stability, StabilitySprite);
    }
}
