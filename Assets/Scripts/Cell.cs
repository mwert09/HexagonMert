using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  We use cell class for raycast hit detection.
 *
 *  
 */
public class Cell : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public Grid m_Grid;

    void Start()
    {

    }

    public void Init(int x, int y, Grid grid)
    {
        xIndex = x;
        yIndex = y;
        m_Grid = grid;
    }
}
