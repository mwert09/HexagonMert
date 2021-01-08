using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager instance;

    [SerializeField] private bool gameEnd = false;
    [SerializeField] private bool paused = false;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Pauses everything and shows the end game panel */
    public void SetGameEnd(string reason)
    {
        gameEnd = true;
        paused = true;
        // Activate panel
        UIManager.instance.gameOverText.text = reason;
        UIManager.instance.ShowGameOverPanel();
    }

    public void SetPaused(bool value)
    {
        paused = value;
    }

    public bool isPaused()
    {
        return paused;
    }

}
