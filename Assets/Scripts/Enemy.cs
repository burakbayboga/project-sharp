using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
	public Animator animator;
	public SpriteRenderer rend;

    public Image CurrentActionImage;
    public Image CurrentReactionImage;
    public Image[] ExposedWeaknessImages;

    public GameObject VulnerableIcon;

    public Color ExposedWeaknessColor;
    public Color NotExposedWeaknessColor;

	public Sprite[] skillSprites;

    public Skill CurrentAction;

    public int TotalDurability;
	public Hex currentHex;

    int CurrentWeaknessExposed;

    int CurrentWeaknessCue;
    public bool IsVulnerable { get; private set; }

    Coroutine WeaknessCueCoroutine;

    void Awake()
    {
        CurrentWeaknessExposed = 0;
        CurrentWeaknessCue = 0;
        IsVulnerable = false;
    }

    private void Start()
    {
        InitWeaknessIcons();
    }

	public void SetRendererFlip(bool flip)
	{
		rend.flipX = flip;
	}

	public void MoveTurn()
	{
		if (currentHex.IsAdjacentToPlayer() || Random.Range(0f, 1f) < 0.55f)
		{
			return;
		}

		Hex newHex = GetHexCloserToPlayer();
		currentHex.isOccupiedByEnemy = false;
		transform.position = newHex.transform.position + Hex.posOffset;
		newHex.isOccupiedByEnemy = true;
		currentHex = newHex;
	}

	Hex GetHexCloserToPlayer()
	{
		Hex closestHex = currentHex;
		Hex playerHex = Player.instance.currentHex;
		float closestDistance = Vector3.Distance(closestHex.transform.position, playerHex.transform.position);

		for (int i = 0; i < currentHex.adjacents.Length; i++)
		{
			Hex traverse = currentHex.adjacents[i];
			if (traverse.isOccupied)
			{
				continue;
			}

			float traverseDistance = Vector3.Distance(traverse.transform.position, playerHex.transform.position);
			if (traverseDistance < closestDistance)
			{
				closestDistance = traverseDistance;
				closestHex = traverse;
			}
		}


		return closestHex;
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

	bool HasLosToPlayer()
	{
		Ray ray = new Ray(transform.position, Player.instance.transform.position - transform.position);
		float distance = Vector3.Distance(Player.instance.transform.position, transform.position);
		if (Physics2D.RaycastAll(ray.origin, ray.direction, distance, 1 << 10).Length == 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

    SkillType GetActionType()
    {
		if (!currentHex.IsAdjacentToPlayer())
		{
			if (HasLosToPlayer())
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

    public void RegisterAction()
    {
		SkillType action = GetActionType();
		CurrentAction = Skill.GetSkillForType(action);

		if (CurrentAction == null)
		{
			// TODO: まじか...
			return;
		}

		CurrentActionImage.sprite = skillSprites[(int)action];

		Color color = CurrentActionImage.color;
		color.a = 1f;
		CurrentActionImage.color = color;

		GameController.instance.RegisterEnemyAction(this, CurrentAction);
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

    public void SetReactionImageAndWeaknessCue(SkillType reactionType, int damage)
    {
        SetReactionImage(reactionType);
        StopPreviousWeaknessCue();
        CurrentWeaknessCue = damage;
        WeaknessCueCoroutine = StartCoroutine(WeaknessCue());
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
