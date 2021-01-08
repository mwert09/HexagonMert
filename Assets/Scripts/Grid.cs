using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  In the future we might create multiple grids like in candy crush but we need to change GridUtil class functions.
 *
 */
public class Grid : MonoBehaviour
{

    // We need to have start positions for each grid so we can properly instantiate
    public float startPosX;
    public float startPosY;

    // Grid width and height to initialize default(8x9)
    public int width, height;

    // Basic Prefab for cell. We will use those to detect ray hits
    public GameObject cellPrefab;

    // Cell array in this grid
    public Cell[,] m_allCells;

    // Hexagon holder array to hold hexagons. Hexagons are basically game pieces we need for core gameplay
    public HexagonHolder[,] m_allHexagons;

    // We have to change our x and y values based on an offset to create our grid
    public float xOffset = 0.759f;
    public float yOffset = -0.44f;

    public Cell m_clickedCell;
    public HexagonHolder[] m_selectedHexagonGroup;

    private void Awake()
    {
        // Initialize cell array
        m_allCells = new Cell[width, height];
        m_allHexagons = new HexagonHolder[width, height];
        m_selectedHexagonGroup = new HexagonHolder[3];
        m_selectedHexagonGroup[0] = null;
        m_selectedHexagonGroup[1] = null;
        m_selectedHexagonGroup[2] = null;
    }

    /* Grid initialization function. Instantiates cell gameobjects and displays them
     */
    public void InitGrid(float startPosX, float startPosY)
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                // find new cell coordinates to instantiate
                float yPos = i * yOffset * 2 + startPosY;
                
                //if it is odd
                if (j % 2 == 1)
                {
                    yPos += yOffset;
                }

                GameObject cell = Instantiate(cellPrefab, new Vector3(j * xOffset + startPosX, yPos, 0), Quaternion.identity);
                cell.name = "Cell " + i + ", " + j;
                m_allCells[j, i] = cell.GetComponent<Cell>();
                cell.transform.parent = transform;
                // fill all cells array and initialize cell (set x, y coordinates and grid)
                try
                {
                    m_allCells[j, i].Init(i, j, this);
                }
                catch (NullReferenceException ex)
                {
                    Debug.Log("Something went wrong with Cell array" + ex.ToString());
                }
            }
        }
    }

    // Returns the whole space occupied on the x axis by this grid
    public float GetWidth()
    {
        return xOffset * width;
    }

    // Returns the whole space occupied on the y axis by this grid
    public float GetHeight()
    {
        return -yOffset * height;
    }

    
}
