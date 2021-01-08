using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public float startPosX;
    public float startPosY;

    // Grid width and height to initialize default(8x9)
    public int width, height;
    // Basic Prefab to test out grid
    public GameObject cellPrefab;

    // Cell array in this grid
    public Cell[,] m_allCells;
    // Hexagon holder array to hold hexagons
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Grid initialization function. Instantiates cell gameobjects and displays it
     */
    public void InitGrid(float startPosX, float startPosY)
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
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

    public float GetWidth()
    {
        return xOffset * width;
    }

    public float GetHeight()
    {
        return -yOffset * height;
    }

    
}
