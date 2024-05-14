using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void NextScene()
    {
        if(SceneManager.GetActiveScene().buildIndex == 2)
        {
            //Debug.Log(UIController.level);
            SceneManager.LoadScene(UIController.level + 2);
        }
        else
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentIndex + 1);
        }
    }
}
