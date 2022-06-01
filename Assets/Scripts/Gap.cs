using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gap : MonoBehaviour
{
    public SpriteRenderer rend;

	void Awake()
	{
		rend.enabled = false;
	}
}
