using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{

    public GameObject[] FocusCostIcons;
    public GameObject[] StrengthCostIcons;
    public GameObject[] StabilityCostIcons;
	public Animator animator;
	public GameObject resourcesParent;

    public GameObject[] DamageIcons;

	public SkillType skillType;

	public GameObject hoverText;

    int CostIconCount;

    public Resource Cost { get; private set; }
    public int Damage { get; private set; }
	public bool canUse;

	RectTransform rectT;
    
    void Awake()
    {
        CostIconCount = FocusCostIcons.Length;
		rectT = GetComponent<RectTransform>();
    }

	public void SetPosition(Vector3 pos)
	{
		rectT.anchoredPosition = pos;
	}

    public void HandleCostAndDamage(Resource cost, int damage, bool setKillingBlowIndicator, bool beingGrappled)
    {
        Cost = cost;
        HandleCostIcons();

        Damage = damage;
        HandleDamageIcons();

		canUse = Cost <= Player.instance.CurrentResource;
		if (beingGrappled && (skillType == SkillType.Sidestep || skillType == SkillType.Wrestle || skillType == SkillType.Charge || skillType == SkillType.Jump))
		{
			canUse = false;
		}

		if (!canUse)
		{
			animator.Play("skill button not enough");
		}
		else if (setKillingBlowIndicator)
		{
			animator.Play("killing blow indicator");
		}
		else
		{
			if (skillType == SkillType.Sidestep || skillType == SkillType.Wrestle || skillType == SkillType.Charge || skillType == SkillType.Jump)
			{
				animator.Play("skill button base purple");
			}
			else
			{
				animator.Play("skill button base");
			}
		}
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
		Player.instance.OnSkillClicked(this);
	}

	public void OnMouseButtonDown()
	{
		GameController.instance.OnMouseButtonDownOnSkill();
	}

	void OnDisable()
	{
		hoverText.SetActive(false);
		resourcesParent.SetActive(false);
	}

	public void OnMouseButtonEnter()
	{
		hoverText.SetActive(true);
		transform.SetAsLastSibling();
		resourcesParent.SetActive(true);
		if (skillType != SkillType.Sidestep && skillType != SkillType.Jump)
		{
			GameController.instance.OnMouseButtonEnterOnSkill(skillType);
		}
	}

	public void OnMouseButtonExit()
	{
		hoverText.SetActive(false);
		resourcesParent.SetActive(false);
		if (skillType != SkillType.Sidestep && skillType != SkillType.Jump)
		{
			GameController.instance.OnMouseButtonExitOnSkill(skillType);
		}
	}

}
