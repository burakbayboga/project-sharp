using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
	public Animator animator;
	public SpriteRenderer rend;
	public GameObject tooltip;
	public TextMeshProUGUI stateText;
	public TextMeshProUGUI actionText;
	float yLimit = 1.66f;
	Vector3 tooltipBasePos;

	public GameObject actionBg;
	public GameObject reactionBg;
    public Image CurrentActionImage;
    public Image CurrentReactionImage;
    public Image[] ExposedWeaknessImages;

    public GameObject VulnerableIcon;

    public Color ExposedWeaknessColor;
    public Color NotExposedWeaknessColor;

	public Sprite[] skillSprites;

    public Skill CurrentAction;
	public Skill CurrentPlayerReaction;
	public bool answeredThisTurn;
	public bool isGrappling;

    public int TotalDurability;
	public Hex currentHex;

    public int CurrentWeaknessExposed;

    int CurrentWeaknessCue;
    public bool IsVulnerable { get; private set; }

	EnemyType enemyType;

    Coroutine WeaknessCueCoroutine;

    void Awake()
    {
        CurrentWeaknessExposed = 0;
        CurrentWeaknessCue = 0;
        IsVulnerable = false;
		enemyType = GetComponent<EnemyType>();
		tooltipBasePos = tooltip.transform.localPosition;
    }

	public void OnMouseEnter()
	{
		tooltip.SetActive(true);
		if (transform.position.y > yLimit)
		{
			Vector3 pos = tooltipBasePos;
			pos.y = pos.y + (yLimit - transform.position.y) * 1.66f;	// multiplier: enemy scale is not unit
			tooltip.transform.localPosition = pos;
		}
		else
		{
			tooltip.transform.localPosition = tooltipBasePos;
		}
	}
	
	public void OnMouseExit()
	{
		tooltip.SetActive(false);
	}

	public void Init(Hex spawnHex)
	{
		enemyType.Init(this);
		currentHex = spawnHex;
		currentHex.enemy = this;
		InitWeaknessIcons();
	}

	public void SetRendererFlip(bool flip)
	{
		rend.flipX = flip;
	}

	public void MoveTurn()
	{
		enemyType.MoveTurn();
	}

	public void MoveToHex(Hex hex, bool forced = false)
	{
		currentHex.enemy = null;
		StartCoroutine(Run(hex.transform.position + Hex.posOffset, forced));
		hex.enemy = this;
		currentHex = hex;
	}

	IEnumerator Run(Vector3 target, bool forced)
	{
		Vector3 start = transform.position;
		float startTime = Time.time;
		float t = 0f;
		SetRendererFlip(target.x < start.x);

		if (!forced)
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

	public Hex GetHexFurtherToPlayer(bool withLos = false)
	{
		List<Hex> candidates = new List<Hex>();
		Hex playerHex = Player.instance.currentHex;
		float currentDistance = Vector3.Distance(currentHex.transform.position, playerHex.transform.position);

		for (int i = 0; i < currentHex.adjacents.Length; i++)
		{
			Hex traverse = currentHex.adjacents[i];
			if (traverse.isOccupied)
			{
				continue;
			}

			float traverseDistance = Vector3.Distance(traverse.transform.position, playerHex.transform.position);
			if (traverseDistance > currentDistance)
			{
				if (withLos && !HasLosToPlayer(traverse))
				{
					continue;
				}

				candidates.Add(traverse);
			}
		}

		if (candidates.Count > 0)
		{
			return candidates[Random.Range(0, candidates.Count)];
		}
		else
		{
			return null;
		}
	}

	public Hex GetHexCloserToPlayer(bool enforceLos = false, bool closest = false)
	{
		List<Hex> candidates = new List<Hex>();
		Hex playerHex = Player.instance.currentHex;
		float currentDistance = Vector3.Distance(currentHex.transform.position, playerHex.transform.position);

		for (int i = 0; i < currentHex.adjacents.Length; i++)
		{
			Hex traverse = currentHex.adjacents[i];
			if (traverse.isOccupied)
			{
				continue;
			}

			float traverseDistance = Vector3.Distance(traverse.transform.position, playerHex.transform.position);
			if (traverseDistance < currentDistance)
			{
				if (enforceLos && !HasLosToPlayer(traverse))
				{
					continue;
				}
				if (closest)
				{
					currentDistance = traverseDistance;
					candidates.Clear();
				}
				candidates.Add(traverse);
			}
		}

		if (candidates.Count > 0)
		{
			return candidates[Random.Range(0, candidates.Count)];
		}
		else
		{
			return null;
		}
	}

	public Hex GetHexWithLosToPlayer()
	{
		Hex[] adjacents = currentHex.adjacents;
		List<Hex> candidates = new List<Hex>();
		for (int i = 0; i < adjacents.Length; i++)
		{
			if (adjacents[i].isOccupied)
			{
				continue;
			}

			if (HasLosToPlayer(adjacents[i]))
			{
				candidates.Add(adjacents[i]);
			}
		}

		if (candidates.Count > 0)
		{
			return candidates[Random.Range(0, candidates.Count)];
		}
		else
		{
			return null;
		}
	}

    void InitWeaknessIcons()
    {
        for (int i = 0; i < ExposedWeaknessImages.Length; i++)
        {
            if (i >= TotalDurability)
            {
                ExposedWeaknessImages[i].gameObject.SetActive(false);
            }
        }
    }

	public bool HasLosToPlayer(Hex hex)
	{
		Ray ray = new Ray(hex.transform.position, Player.instance.currentHex.transform.position - hex.transform.position);
		float distance = Vector3.Distance(Player.instance.currentHex.transform.position, hex.transform.position);
		if (Physics2D.RaycastAll(ray.origin, ray.direction, distance, 1 << 10).Length == 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	
	public void ForceCancelAction()
	{
		if (answeredThisTurn)
		{
			return;
		}
		CurrentAction = null;
		ResetIcons();
		SetTooltipText();
	}

	public void CheckActionValidity()
	{
		if (answeredThisTurn)
		{
			return;
		}
		if (CurrentAction == Skill.ShootArrow)
		{
			if (!HasLosToPlayer(currentHex))
			{
				CurrentAction = null;
				ResetIcons();
			}
		}
		else if (CurrentAction == Skill.Skewer)
		{
			if (!HasLosToPlayer(currentHex) || Vector3.Distance(currentHex.transform.position, Player.instance.currentHex.transform.position) > 2f)
			{
				CurrentAction = null;
				ResetIcons();
			}
		}
		else if (CurrentAction != null && !IsDefensive())
		{
			if (!currentHex.IsAdjacentToPlayer())
			{
				CurrentAction = null;
				ResetIcons();
			}
		}

		SetTooltipText();
	}

	public bool IsDefensive()
	{
		return CurrentAction == Skill.Block || CurrentAction == Skill.Counter;
	}

	void SetTooltipText(bool reset = false)
	{
		if (reset)
		{
			stateText.text = "";
			actionText.text = "";
		}
		else if (CurrentAction == null)
		{
			stateText.text = "<color=green>Idle</color>";
			actionText.text = "";
		}
		else if (CurrentAction == Skill.SwiftAttack)
		{
			stateText.text = "<color=red>Attacking</color>";
			actionText.text = "Swift Attack (<sprite=1>)";
		}
		else if (CurrentAction == Skill.HeavyAttack)
		{
			stateText.text = "<color=red>Attacking</color>";
			actionText.text = "Heavy Attack (<sprite=\"roguelikeChar_transparent\" index=0>)";
		}
		else if (CurrentAction == Skill.Block)
		{
			stateText.text = "<color=blue>Defending</color>";
			actionText.text = "Block (<sprite=\"roguelikeChar_transparent\" index=1>)";
		}
		else if (CurrentAction == Skill.Counter)
		{
			stateText.text = "<color=blue>Defending</color>";
			actionText.text = "Counter (<sprite=\"colored_transparent\" index=2>)";
		}
		else if (CurrentAction == Skill.ShootArrow)
		{
			stateText.text = "<color=red>Attacking</color>";
			actionText.text = "Shoot Arrow (<sprite=\"colored_transparent\" index=11>)";
		}
		else if (CurrentAction == Skill.Skewer)
		{
			stateText.text = "<color=red>Attacking</color>";
			actionText.text = "Skewer (<sprite=\"colored_transparent\" index=10>)";
		}
		else if (CurrentAction == Skill.Grapple)
		{
			stateText.text = "<color=red>Attacking</color>";
			actionText.text = "Grapple (<sprite=\"colored_transparent\" index=20>)";
		}
	}

    public void PickAction()
    {
		SkillType action = enemyType.GetActionType();
		CurrentAction = Skill.GetSkillForType(action);
		SetTooltipText();

		if (CurrentAction == null)
		{
			return;
		}


		CurrentActionImage.sprite = skillSprites[(int)action];

		Color color = CurrentActionImage.color;
		color.a = 1f;
		CurrentActionImage.color = color;

		actionBg.SetActive(true);
    }

    public void ExposeWeakness(int exposeAmount)
    {
        if (!IsVulnerable)
        {
            CurrentWeaknessExposed += exposeAmount;

            if (CurrentWeaknessExposed >= TotalDurability)
            {
                CurrentWeaknessExposed = TotalDurability;
                SetVulnerable(true);
            }

            UpdateWeaknessImages();
        }
    }

	public void CoverWeakness(int coverAmount)
	{
		CurrentWeaknessExposed = Mathf.Max(0, CurrentWeaknessExposed - coverAmount);
		if (IsVulnerable)
		{
			SetVulnerable(false);
		}
		UpdateWeaknessImages();
	}

	void UpdateWeaknessImages()
	{
		StopPreviousWeaknessCue();
		for (int i = 0; i < ExposedWeaknessImages.Length; i++)
		{
			if (i >= CurrentWeaknessExposed)
			{
				ExposedWeaknessImages[i].color = NotExposedWeaknessColor;
			}
			else
			{
				ExposedWeaknessImages[i].color = ExposedWeaknessColor;
			}
		}
	}

    void SetVulnerable(bool vulnerable)
    {
        IsVulnerable = vulnerable;
        VulnerableIcon.SetActive(vulnerable);

		for (int i = 0; i < TotalDurability; i++)
		{
			ExposedWeaknessImages[i].gameObject.SetActive(!vulnerable);
		}
    }

	public void ResetEnemy()
	{
		ResetIcons();
		CurrentAction = null;
		CurrentPlayerReaction = null;
		answeredThisTurn = false;
		SetTooltipText(true);
		isGrappling = false;
	}

    public void ResetIcons()
    {
        Color color = CurrentReactionImage.color;
        color.a = 0f;
        CurrentReactionImage.color = color;

		reactionBg.SetActive(false);

        color = CurrentActionImage.color;
        color.a = 0f;
        CurrentActionImage.color = color;

		actionBg.SetActive(false);

        StopPreviousWeaknessCue();
    }

	public void SetPlayerReaction(Skill reaction, int damage)
	{
		CurrentPlayerReaction = reaction;
		isGrappling = false;
		SkillType reactionType = reaction == null ? SkillType.None : reaction.Type;
		SetReactionImage(reactionType);
		StopPreviousWeaknessCue();
		CurrentWeaknessCue = damage;
		if (damage > 0)
		{
			WeaknessCueCoroutine = StartCoroutine(WeaknessCue());
		}
	}

    void StopPreviousWeaknessCue()
    {
        if (WeaknessCueCoroutine != null)
        {
            StopCoroutine(WeaknessCueCoroutine);

            for (int i = 0; i < TotalDurability; i++)
            {
                if (i >= CurrentWeaknessExposed && i < CurrentWeaknessExposed + CurrentWeaknessCue)
                {
                    ExposedWeaknessImages[i].color = NotExposedWeaknessColor;
                }
            }
        }
    }

    IEnumerator WeaknessCue()
    {
		yield break;
        while (true)
        {
            for (int i = CurrentWeaknessExposed; i < CurrentWeaknessExposed + CurrentWeaknessCue; i++)
            {
                ExposedWeaknessImages[i].color = Color.Lerp(ExposedWeaknessColor, NotExposedWeaknessColor, Mathf.PingPong(Time.time * 2f, 1f));
            }

            yield return null;
        }
    }

    void SetReactionImage(SkillType reactionType)
    {

        if (reactionType == SkillType.None)
        {
            Color color = CurrentReactionImage.color;
            color.a = 0f;
            CurrentReactionImage.color = color;

			reactionBg.SetActive(false);
        }
        else
        {
			CurrentReactionImage.sprite = skillSprites[(int)reactionType];
            Color color = CurrentReactionImage.color;
            color.a = 1f;
            CurrentReactionImage.color = color;

			reactionBg.SetActive(true);
        }
    }
}
