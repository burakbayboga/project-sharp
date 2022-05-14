using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
			if (enemy.IsDefensive())
			{
				enemy.CoverWeakness(GetCoveredWeaknessByEnemy());
			}
			else
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

	protected Resource GetItemModifier()
	{
		List<Item> items = Player.instance.items;
		Resource itemModifier = new Resource();
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].modifiedSkillType == Type)
			{
				itemModifier += items[i].resourceModifier;
			}
		}

		return itemModifier;
	}

	public virtual int GetCoveredWeaknessByEnemy() { return 0; }

    public static HeavyAttack HeavyAttack;
    public static SwiftAttack SwiftAttack;
    public static Counter Counter;
    public static Block Block;
    public static KillingBlow KillingBlow;
	public static ShootArrow ShootArrow;
	public static DeflectArrow DeflectArrow;
	public static Skewer Skewer;
	public static BlockArrow BlockArrow;

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
			case SkillType.Skewer:
				return Skewer;
			case SkillType.BlockArrow:
				return BlockArrow;
			default:
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
		Skewer = new Skewer();
		BlockArrow = new BlockArrow();
    }
}

[Serializable]
public struct Resource
{
    public int Focus;
    public int Strength;
    public int Stability;

	public void PrintCost()
	{
		Debug.Log("Foc: " + Focus + "  Str: " + Strength + "  Sta: " + Stability);
	}

	public void Clamp()
	{
		Focus = Mathf.Max(0, Focus);
		Strength = Mathf.Max(0, Strength);
		Stability = Mathf.Max(0, Stability);
	}

    public static Resource operator + (Resource a, Resource b)
    {
        return new Resource
        {
			Focus = a.Focus + b.Focus,
            Strength = a.Strength + b.Strength,
            Stability = a.Stability + b.Stability
        };
    }

    public static Resource operator + (Resource a, int b)
    {
        return new Resource
        {
            Focus = a.Focus + b,
            Strength = a.Strength + b,
            Stability = a.Stability + b
        };
    }

    public static Resource operator / (Resource a, int b)
    {
        return new Resource
        {
            Focus = a.Focus / b,
            Strength = a.Strength / b,
            Stability = a.Stability / b
        };
    }

    public static Resource operator - (Resource a, Resource b)
    {
        return new Resource
        {
            Focus = a.Focus - b.Focus,
            Strength = a.Strength - b.Strength,
            Stability = a.Stability - b.Stability
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
	Skewer = 7,
	BlockArrow = 8,
    None = -1
}
