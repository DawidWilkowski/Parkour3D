using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void playGame()
    {
        //Time.timeScale = 1;
        SceneManager.LoadScene("SampleScene");

    }
    public void quitGame()
    {
        Application.Quit();
    }
}
