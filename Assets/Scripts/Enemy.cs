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
    public GameObject Highlight;

    public Color ExposedWeaknessColor;
    public Color NotExposedWeaknessColor;

	public Sprite[] skillSprites;

    Skill CurrentAction;

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

	void GoTowardsPlayer()
	{

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

    SkillType GetActionType()
    {
		if (!currentHex.IsAdjacentToPlayer())
		{
			return SkillType.ShootArrow;
		}
        if (IsVulnerable)
        {
            int random = Random.Range(0, 6);
            if (random == 0)
            {
                return SkillType.HeavyAttack;
            }
            else if (random == 1)
            {
                return SkillType.SwiftAttack;
            }
            else if (random < 4)
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
            return (SkillType)Random.Range(0, 4);
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
                MakeVulnerable();
            }

            for (int i = 0; i < CurrentWeaknessExposed; i++)
            {
                ExposedWeaknessImages[i].color = ExposedWeaknessColor;
            }
        }
    }

    void MakeVulnerable()
    {
        IsVulnerable = true;
        VulnerableIcon.SetActive(true);
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
