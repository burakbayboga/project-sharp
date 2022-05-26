using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteResourceButton : MonoBehaviour
{
    public GameObject tooltip;

	public void OnMouseEnter()
	{
		tooltip.SetActive(true);
	}

	public void OnMouseExit()
	{
		tooltip.SetActive(false);
	}

	public void OnClickFocus()
	{
		tooltip.SetActive(false);
		Player.instance.OnHawkFocusClicked();
	}

	public void OnClickStrength()
	{
		tooltip.SetActive(false);
		Player.instance.OnBullStrengthClicked();
	}

	public void OnClickStability()
	{
		tooltip.SetActive(false);
		Player.instance.OnTurtleStabilityClicked();
	}
}
