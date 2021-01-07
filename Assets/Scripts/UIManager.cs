using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public int scoreAmount = 15;

    public Text scoreText;
    public Text movesText;

    public GameObject pausePanel;

    public int score;
    public int moves;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        moves = 0;
        scoreText.text = score.ToString();
        movesText.text = moves.ToString();
        UpdateUI();
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
        GameFlowManager.instance.paused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeButtonPressed()
    {
        GameFlowManager.instance.paused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void ExitButtonPressed()
    {
        // Exit
        Application.Quit();
    }
}
