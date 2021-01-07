using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager instance;

    public bool gameEnd = false;
    public bool paused = false;

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
        if (gameEnd)
        {

        } 
    }

    public void SetGameEnd()
    {
        gameEnd = true;
        paused = true;
        // Activate panel

    }
}
