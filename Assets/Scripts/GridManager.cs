using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    
    public List<Grid> gridList;

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
}
