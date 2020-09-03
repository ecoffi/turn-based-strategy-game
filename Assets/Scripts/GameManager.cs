using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class GameManager : Singleton<GameManager>
{
    private static GameManager _instance;
    private static bool _playerWon = false; //whether player won the last battle or not
    
    public void SetPlayerWon(bool playerWon)
    {
        _playerWon = playerWon;
    }

    public bool GetPlayerWon()
    {
        return _playerWon;
    }

    // Start is called before the first frame update
    private void Start()
    {
        //GameManager persists across all scenes 
        DontDestroyOnLoad(gameObject);
    }

    public static void LoadScene(String sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
