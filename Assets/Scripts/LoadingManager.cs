using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    
	void Start()
	{
		if (PlayerPrefs.GetInt("seenTutorial", 0) == 0)
		{
			SceneManager.LoadScene("tutorial");
		}
		else
		{
			SceneManager.LoadScene("game");
		}
	}
}
