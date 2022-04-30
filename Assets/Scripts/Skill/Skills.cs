﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public Resource BaseCost;
    public SkillType Type;

    //this is called for every enemy action. param:reaction refers to player action
    public virtual void HandleClash(Enemy enemy, SkillType playerReaction) { }

    // player calls this for every skill. param:action refers to enemy action
    public virtual Resource GetTotalCost(SkillType enemyAction, out int damage) { damage = 0; return new Resource(); }

    public static HeavyAttack HeavyAttack;
    public static SwiftAttack SwiftAttack;
    public static Counter Counter;
    public static Block Block;
    public static KillingBlow KillingBlow;

    public static void InitSkills()
    {
        HeavyAttack = new HeavyAttack();
        SwiftAttack = new SwiftAttack();
        Counter = new Counter();
        Block = new Block();
        KillingBlow = new KillingBlow();
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
    None = -1
}
