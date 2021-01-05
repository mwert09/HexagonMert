﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public List<Grid> gridList;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitGrids();
    }

    private void InitGrids()
    {
        int lengthOfGridList = gridList.Capacity;
        for (int i = 0; i < lengthOfGridList; i++)
        {
            gridList[i].InitGrid(gridList[i].startPosX, gridList[i].startPosY);
        }
    }

    public float GetWidthSum()
    {
        float widthSum = 0;
        int lengthOfGridList = gridList.Capacity;
        for (int i = 0; i < lengthOfGridList; i++)
        {
            widthSum += gridList[i].GetWidth();
        }

        return widthSum;
    }

    public float GetHeightSum()
    {
        float heightSum = 0;
        int lengthOfGridList = gridList.Capacity;
        for (int i = 0; i < lengthOfGridList; i++)
        {
            heightSum += gridList[i].GetHeight();
        }

        return heightSum;
    }
}
