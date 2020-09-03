using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class MainMenuController : Singleton<MainMenuController>
{
    /// <summary>
    /// Start playing the game
    /// </summary>
    public void Play()
    {
        GameManager.LoadScene("Game");
    }

    /// <summary>
    /// quit the application
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
}
