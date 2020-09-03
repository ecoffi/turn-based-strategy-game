using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinnerMenuController : MonoBehaviour
{
    private GameManager _gameManager;
    [SerializeField] private Text winnerText;

    public void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        SetWinnerText();
    }

    /// <summary>
    /// Set The text of the winner text object
    /// </summary>
    private void SetWinnerText()
    {
        winnerText.text = _gameManager.GetPlayerWon() ? "You Won!" : "You Lost!";
    }
    
    /// <summary>
    /// return to main menu
    /// </summary>
    public void ReturnToMenu()
    {
        GameManager.LoadScene("MainMenu");
    }
}
