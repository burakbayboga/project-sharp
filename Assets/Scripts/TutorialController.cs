using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public void OnBackToGameClicked()
    {
        SceneManager.LoadScene("game");
    }

	//void Update()
	//{
		//if (Input.GetKeyDown(KeyCode.Space))
		//{
			//print("ss");
			//ScreenCapture.CaptureScreenshot("tutorial image.png");
		//}
	//}
}
