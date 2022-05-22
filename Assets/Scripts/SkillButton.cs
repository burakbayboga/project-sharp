using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillButton : MonoBehaviour
{

    public GameObject[] FocusCostIcons;
    public GameObject[] StrengthCostIcons;
    public GameObject[] StabilityCostIcons;

    public GameObject[] DamageIcons;

	public SkillType skillType;

	public GameObject hoverText;

    int CostIconCount;

    public Resource Cost { get; private set; }
    public int Damage { get; private set; }
    
    void Awake()
    {
        CostIconCount = FocusCostIcons.Length;
    }

    public void HandleCostAndDamage(Resource cost, int damage)
    {
        Cost = cost;
        HandleCostIcons();

        Damage = damage;
        HandleDamageIcons();
    }

    void HandleDamageIcons()
    {
        for (int i = 0; i < DamageIcons.Length; i++)
        {
            if (i < Damage)
            {
                DamageIcons[i].SetActive(true);
            }
            else
            {
                DamageIcons[i].SetActive(false);
            }
        }
    }

    void HandleCostIcons()
    {
        for (int i = 0; i < CostIconCount; i++)
        {
            if (i < Cost.Focus)
            {
                FocusCostIcons[i].SetActive(true);
            }
            else
            {
                FocusCostIcons[i].SetActive(false);
            }

            if (i < Cost.Strength)
            {
                StrengthCostIcons[i].SetActive(true);
            }
            else
            {
                StrengthCostIcons[i].SetActive(false);
            }

            if (i < Cost.Stability)
            {
                StabilityCostIcons[i].SetActive(true);
            }
            else
            {
                StabilityCostIcons[i].SetActive(false);
            }
        }
    }

	public void OnSkillClicked()
	{
		Player.instance.OnSkillClicked(skillType);
	}

	public void OnMouseButtonDown()
	{
		GameController.instance.OnMouseButtonDownOnSkill();
	}

	void OnDisable()
	{
		hoverText.SetActive(false);
	}

	public void OnMouseButtonEnter()
	{
		hoverText.SetActive(true);
		if (skillType != SkillType.Sidestep)
		{
			GameController.instance.OnMouseButtonEnterOnSkill(skillType);
		}
	}

	public void OnMouseButtonExit()
	{
		hoverText.SetActive(false);
		if (skillType != SkillType.Sidestep)
		{
			GameController.instance.OnMouseButtonExitOnSkill(skillType);
		}
	}

}
