using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public List<Grid> gridList;
    public GameObject[] hexagonPrefabs;

    public List<Color> colorList;

    public List<HexagonHolder> bombList;

    private void Awake()
    {
        instance = this;
        /*for (int i = 0; i < colorList.Count; i++)
        {
            Color newColor = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
            );
            colorList[i] = newColor;
        }*/
    }

    // Start is called before the first frame update
    void Start()
    {
        InitGrids();
        FillGridsWithRandom();
       GridUtils.instance.ExplodeStartingMatches(false);
       if (!GridUtils.instance.CheckPossibleMoves())
       {
           Debug.Log("Restarting");
            LevelManager.instance.LoadLevel(1);
       }
    }


    private void FillGridsWithRandom()
    {
        for (int i = 0; i < gridList.Capacity; i++)
        {
            GridUtils.instance.FillRandom(i);
        }
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
