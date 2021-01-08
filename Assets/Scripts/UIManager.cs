using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [Header("Score")]
    public int scoreAmount = 15;
    public int score;

    [Header("Moves")]
    public int moves;

    [Header("UI Objects")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI gameOverScoreText;
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        moves = 0;
        try
        {
            scoreText.text = score.ToString();
            movesText.text = moves.ToString();
            UpdateUI();
        }
        catch (NullReferenceException ex)
        {
            Debug.Log(ex.ToString());
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseScore()
    {
        score += scoreAmount;
        UpdateUI();
    }

    public void IncreaseMoves()
    {
        moves++;
        UpdateUI();
    }

    public void UpdateUI()
    {
        scoreText.text = score.ToString();
        movesText.text = moves.ToString();
    }

    public void PauseButtonPressed()
    {
        GameFlowManager.instance.SetPaused(true);
        pausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeButtonPressed()
    {
        GameFlowManager.instance.SetPaused(false);
        pausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void MainMenuPressed()
    {
        LevelManager.instance.LoadLevel(0);
    }

    public void ShowGameOverPanel()
    {
        gameOverScoreText.text = score.ToString();
        gameOverPanel.SetActive(true);
    }

    public void RestartButtonPressed()
    {
        LevelManager.instance.LoadLevel(1);
    }

    public void ExitButtonPressed()
    {
        // Exit
        Application.Quit();
    }
}
