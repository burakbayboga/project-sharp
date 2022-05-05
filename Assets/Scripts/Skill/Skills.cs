using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public Resource BaseCost;
    public SkillType Type;
	public int clip;

    //this is called for every enemy action. param:reaction refers to player action
    public virtual void HandleClash(Enemy enemy, Skill playerReaction)
	{
		if (playerReaction == null)
		{
			if (!enemy.IsDefensive())
			{
				Player.instance.GetInjury();
			}
		}
		else if (playerReaction == Skill.KillingBlow)
		{
			GameController.instance.MarkEnemyForDeath(enemy);
		}
		else
		{
			enemy.ExposeWeakness(playerReaction.GetDamageAgainstEnemyAction(Type));
		}
	}

    // player calls this for every skill. param:action refers to enemy action
    public virtual Resource GetTotalCost(SkillType enemyAction) { return new Resource(); }

	public virtual int GetDamageAgainstEnemyAction(SkillType enemyAction) { return 0; }

    public static HeavyAttack HeavyAttack;
    public static SwiftAttack SwiftAttack;
    public static Counter Counter;
    public static Block Block;
    public static KillingBlow KillingBlow;
	public static ShootArrow ShootArrow;
	public static DeflectArrow DeflectArrow;

	public static Skill GetSkillForType(SkillType type)
	{
		switch (type)
		{
			case SkillType.Block:
				return Block;
			case SkillType.Counter:
				return Counter;
			case SkillType.SwiftAttack:
				return SwiftAttack;
			case SkillType.HeavyAttack:
				return HeavyAttack;
			case SkillType.KillingBlow:
				return KillingBlow;
			case SkillType.ShootArrow:
				return ShootArrow;
			case SkillType.DeflectArrow:
				return DeflectArrow;
			default:
				Debug.Log("something's wrong i can feel it");
				return null;
		}
	}

    public static void InitSkills()
    {
        HeavyAttack = new HeavyAttack();
        SwiftAttack = new SwiftAttack();
        Counter = new Counter();
        Block = new Block();
        KillingBlow = new KillingBlow();
		ShootArrow = new ShootArrow();
		DeflectArrow = new DeflectArrow();
    }
}

public struct Resource
{
    public int Focus;
    public int Strength;
    public int Stability;

    public static Resource operator + (Resource a, Resource b)
    {
        return new Resource
        {
            Focus = Mathf.Clamp(a.Focus + b.Focus, 0, int.MaxValue),
            Strength = Mathf.Clamp(a.Strength + b.Strength, 0, int.MaxValue),
            Stability = Mathf.Clamp(a.Stability + b.Stability, 0, int.MaxValue),
        };
    }

    // skulls for the skull throne
    public static Resource operator + (Resource a, int b)
    {
        return new Resource
        {
            Focus = Mathf.Clamp(a.Focus + b, 0, Player.instance.MaxResource.Focus),
            Strength = Mathf.Clamp(a.Strength + b, 0, Player.instance.MaxResource.Strength),
            Stability = Mathf.Clamp(a.Stability + b, 0, Player.instance.MaxResource.Stability)
        };
    }

    public static Resource operator / (Resource a, int b)
    {
        return new Resource
        {
            Focus = Mathf.Clamp(a.Focus / b, 0, int.MaxValue),
            Strength = Mathf.Clamp(a.Strength / b, 0, int.MaxValue),
            Stability = Mathf.Clamp(a.Stability / b, 0, int.MaxValue)
        };
    }

    public static Resource operator - (Resource a, Resource b)
    {
        return new Resource
        {
            Focus = Mathf.Clamp(a.Focus - b.Focus, 0, int.MaxValue),
            Strength = Mathf.Clamp(a.Strength - b.Strength, 0, int.MaxValue),
            Stability = Mathf.Clamp(a.Stability - b.Stability, 0, int.MaxValue)
        };
    }

    public static bool operator <= (Resource a, Resource b)
    {
        return a.Focus <= b.Focus && a.Strength <= b.Strength && a.Stability <= b.Stability;
    }

    public static bool operator >= (Resource a, Resource b)
    {
        return a.Focus >= b.Focus && a.Strength >= b.Strength && a.Stability >= b.Stability;
    }
}

public enum SkillType
{
    HeavyAttack = 0,
    SwiftAttack = 1,
    Block = 2,
    Counter = 3,
    KillingBlow = 4,
	ShootArrow = 5,
	DeflectArrow = 6,
    None = -1
}
