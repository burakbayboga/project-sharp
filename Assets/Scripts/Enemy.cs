using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    public Image CurrentActionImage;
    public Image CurrentReactionImage;
    public Image[] ExposedWeaknessImages;

    public GameObject VulnerableIcon;
    public GameObject Highlight;

    public Color ExposedWeaknessColor;
    public Color NotExposedWeaknessColor;

    public Sprite SwiftAttackImage;
    public Sprite HeavyAttackImage;
    public Sprite BlockImage;
    public Sprite CounterImage;
    public Sprite KillingBlowImage;

    Skill CurrentAction;

    public int TotalDurability;

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

    public void RegisterAction()
    {
        SkillType action = GetActionType();

        switch (action)
        {
            case SkillType.HeavyAttack:
                CurrentActionImage.sprite = HeavyAttackImage;
                CurrentAction = Skill.HeavyAttack;
                break;
            case SkillType.SwiftAttack:
                CurrentActionImage.sprite = SwiftAttackImage;
                CurrentAction = Skill.SwiftAttack;
                break;
            case SkillType.Block:
                CurrentActionImage.sprite = BlockImage;
                CurrentAction = Skill.Block;
                break;
            case SkillType.Counter:
                CurrentActionImage.sprite = CounterImage;
                CurrentAction = Skill.Counter;
                break;
            default:
                break;
        }

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
        switch (reactionType)
        {
            case SkillType.HeavyAttack:
                CurrentReactionImage.sprite = HeavyAttackImage;
                break;
            case SkillType.SwiftAttack:
                CurrentReactionImage.sprite = SwiftAttackImage;
                break;
            case SkillType.Block:
                CurrentReactionImage.sprite = BlockImage;
                break;
            case SkillType.Counter:
                CurrentReactionImage.sprite = CounterImage;
                break;
            case SkillType.KillingBlow:
                CurrentReactionImage.sprite = KillingBlowImage;
                break;
            default:
                break;
        }

        if (reactionType == SkillType.None)
        {
            Color color = CurrentReactionImage.color;
            color.a = 0f;
            CurrentReactionImage.color = color;
        }
        else
        {
            Color color = CurrentReactionImage.color;
            color.a = 1f;
            CurrentReactionImage.color = color;
        }
    }
}
