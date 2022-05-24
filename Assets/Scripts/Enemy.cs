using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
	public Animator animator;
	public SpriteRenderer rend;
	public Color color;
	public EnemyType type;

    public Image CurrentActionImage;
    public Image CurrentReactionImage;
    public Image[] ExposedWeaknessImages;

    public GameObject VulnerableIcon;

    public Color ExposedWeaknessColor;
    public Color NotExposedWeaknessColor;

	public Sprite[] skillSprites;

    public Skill CurrentAction;
	public Skill CurrentPlayerReaction;
	public Clash CurrentClash;

    public int TotalDurability;
	public Hex currentHex;

    protected int CurrentWeaknessExposed;

    int CurrentWeaknessCue;
    public bool IsVulnerable { get; private set; }

    Coroutine WeaknessCueCoroutine;

    void Awake()
    {
        CurrentWeaknessExposed = 0;
        CurrentWeaknessCue = 0;
        IsVulnerable = false;
    }

	public virtual void Init(Hex spawnHex)
	{
		currentHex = spawnHex;
		currentHex.isOccupiedByEnemy = true;
		InitWeaknessIcons();
	}

	public void SetRendererFlip(bool flip)
	{
		rend.flipX = flip;
	}

	public virtual void MoveTurn()
	{
		if (currentHex.IsAdjacentToPlayer())
		{
			return;
		}

		if (!HasLosToPlayer(currentHex))
		{
			// no line of sight to player
			Hex newHex = GetHexWithLosToPlayer();
			if (newHex != null)
			{
				// go to hex with line of sight to player
				MoveToHex(newHex);
			}
			else
			{
				// no adjacent hex with line of sight to player, try to get closer at least
				newHex = GetHexCloserToPlayer();
				if (newHex != null)
				{
					MoveToHex(newHex);
				}
			}
		}
		else if (Random.Range(0f, 1f) < 0.5f)
		{
			// has line of sight to player, get closer anyway
			Hex newHex = GetHexCloserToPlayer();
			if (newHex != null)
			{
				MoveToHex(newHex);
			}
		}
	}

	public void MoveToHex(Hex hex)
	{
		currentHex.isOccupiedByEnemy = false;
		transform.position = hex.transform.position + Hex.posOffset;
		hex.isOccupiedByEnemy = true;
		currentHex = hex;
	}

	protected Hex GetHexFurtherToPlayer(bool withLos = false)
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

	protected Hex GetHexCloserToPlayer(bool enforceLos = false, bool closest = false)
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

	protected Hex GetHexWithLosToPlayer()
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
		CurrentAction = null;
		ResetIcons();
	}

	public void CheckActionValidity()
	{
		if (CurrentAction == Skill.ShootArrow)
		{
			if (!HasLosToPlayer(currentHex))
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
	}

    protected virtual SkillType GetActionType()
    {
		if (!currentHex.IsAdjacentToPlayer())
		{
			if (HasLosToPlayer(currentHex))
			{
				return SkillType.ShootArrow;
			}
			else
			{
				return SkillType.None;
			}
		}

		if (IsVulnerable)
		{
			int random = Random.Range(0, 2);
			if (random == 0)
			{
				return SkillType.Block;
			}
			else
			{
				return SkillType.Counter;
			}
		}
		else if (CurrentWeaknessExposed > 1)
        {
            int random = Random.Range(0, 10);
            if (random < 2)
            {
                return SkillType.HeavyAttack;
            }
            else if (random < 4)
            {
                return SkillType.SwiftAttack;
            }
            else if (random < 7)
            {
                return SkillType.Block;
            }
            else
            {
                return SkillType.Counter;
            }
        }
        else
        {
            return (SkillType)Random.Range(0, 2);
        }
    }

	public bool IsDefensive()
	{
		return CurrentAction == Skill.Block || CurrentAction == Skill.Counter;
	}

    public void PickAction()
    {
		SkillType action = GetActionType();
		CurrentAction = Skill.GetSkillForType(action);

		if (CurrentAction == null)
		{
			return;
		}

		CurrentActionImage.sprite = skillSprites[(int)action];

		Color color = CurrentActionImage.color;
		color.a = 1f;
		CurrentActionImage.color = color;
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

    public void ResetIcons()
    {
        Color color = CurrentReactionImage.color;
        color.a = 0f;
        CurrentReactionImage.color = color;

        color = CurrentActionImage.color;
        color.a = 0f;
        CurrentActionImage.color = color;

        StopPreviousWeaknessCue();
    }

	public void SetPlayerReaction(Skill reaction, int damage)
	{
		CurrentPlayerReaction = reaction;
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
        }
        else
        {
			CurrentReactionImage.sprite = skillSprites[(int)reactionType];
            Color color = CurrentReactionImage.color;
            color.a = 1f;
            CurrentReactionImage.color = color;
        }
    }
}

public enum EnemyType
{
	basic,
	archer,
	brute
}
