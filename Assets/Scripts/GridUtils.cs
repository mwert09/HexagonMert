using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using Random = UnityEngine.Random;

/* This class has bunch of functions for gameplay mechanics */
public class GridUtils : MonoBehaviour
{
    public static GridUtils instance;

    public bool alreadyRotating = false;
    public int bombScore = 1000;

    // Enums for hexagon group selection order
    enum Selection
    {
        TOP_LEFT = 1,
        TOP_RIGHT = 2,
        LEFT = 3,
        RIGHT = 4,
        BOTTOM_LEFT = 5,
        BOTTOM_RIGHT = 6
    }

    private Selection selection;

    // Store previosly selected hexagon group to destroy it's outline
    private HexagonHolder[] previouslySelectedHexagonGroup;

    // Struct to hold locations of neighbouring hexagons 
    public struct NeighbourHexagons
    {
        public Vector2 top;
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottom;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }

    // this holds hexagon pair with the same color
    public struct HexagonPair
    {
        public HexagonHolder first;
        public HexagonHolder second;
    }

    // Holds array of hexagons that will explode
    HexagonHolder[] explosionList = new HexagonHolder[3];

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // initial values for previously selected hexagon group members
        previouslySelectedHexagonGroup = new HexagonHolder[3];
        previouslySelectedHexagonGroup[0] = GridManager.instance.gridList[0].m_allHexagons[0, 0];
        previouslySelectedHexagonGroup[1] = GridManager.instance.gridList[0].m_allHexagons[0, 1];
        previouslySelectedHexagonGroup[2] = GridManager.instance.gridList[0].m_allHexagons[1, 1];
    }

    // Function to get neighbours of a hexagon
    private NeighbourHexagons GetNeighbourHexagons(HexagonHolder selectedHexagon)
    {
        //(selectedHexagon.yIndex % 2 == 0) ? selectedHexagon.yIndex - 1 : selectedHexagon.yIndex
        NeighbourHexagons neighbours;
        neighbours.top = new Vector2(selectedHexagon.xIndex, selectedHexagon.yIndex - 1);
        neighbours.bottom = new Vector2(selectedHexagon.xIndex, selectedHexagon.yIndex + 1);
        neighbours.topLeft = new Vector2(selectedHexagon.xIndex - 1, ((float)(selectedHexagon.xIndex % 2) == 0) ? selectedHexagon.yIndex - 1 : selectedHexagon.yIndex);
        neighbours.topRight = new Vector2(selectedHexagon.xIndex + 1, ((float)(selectedHexagon.xIndex % 2) == 0) ? selectedHexagon.yIndex - 1 : selectedHexagon.yIndex);
        neighbours.bottomLeft = new Vector2(selectedHexagon.xIndex - 1, ((float)(selectedHexagon.xIndex % 2) == 0) ? selectedHexagon.yIndex : selectedHexagon.yIndex + 1);
        neighbours.bottomRight = new Vector2(selectedHexagon.xIndex + 1, ((float)(selectedHexagon.xIndex % 2) == 0) ? selectedHexagon.yIndex : selectedHexagon.yIndex + 1);
        return neighbours;
    }

    // Returns a hexagon with a random color selected from the color list
    public GameObject GetRandomPiece()
    {
        int randomColorIndex = Random.Range(0, GridManager.instance.colorList.Count);
        int randomPrefabIndex = Random.Range(0, GridManager.instance.hexagonPrefabs.Length);
        if (GridManager.instance.colorList[randomColorIndex] == null)
        {
            Debug.Log("Grid: " + randomColorIndex + "does not contain a valid color");
        }

        if (GridManager.instance.hexagonPrefabs[randomPrefabIndex] == null)
        {
            Debug.Log("Grid: " + randomPrefabIndex + "does not contain a valid prefab");
        }

        GameObject hexagonToSpawn = GridManager.instance.hexagonPrefabs[randomPrefabIndex];
        hexagonToSpawn.GetComponent<HexagonHolder>().color = GridManager.instance.colorList[randomColorIndex];
        return hexagonToSpawn;
    }

    // Sets hexagon x and y coordinates
    public void PlaceGamePiece(HexagonHolder hexagon, float xPos, float yPos, int x, int y)
    {
        try
        {
            hexagon.transform.position = new Vector3(xPos, yPos, 0);
            hexagon.transform.rotation = Quaternion.identity;
            hexagon.SetCoord(x, y);

        }
        catch (NullReferenceException ex)
        {
            Debug.Log(ex.ToString());
            return;
        }
        
    }

    // Fills grid with random hexagons
    public void FillRandom(int IndexForGrid)
    {
        int gridWidth = GridManager.instance.gridList[IndexForGrid].width;
        int gridHeight = GridManager.instance.gridList[IndexForGrid].height;

        // Loop through the grid, find the correct x and y positions in the world and instantiate a hexagon with those values
        // Also don't forget to initialize all hexagons array with i and j values for the given grid
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                float yPos = i * GridManager.instance.gridList[IndexForGrid].yOffset * 2 + GridManager.instance.gridList[IndexForGrid].startPosY;
                float xOffset = GridManager.instance.gridList[IndexForGrid].xOffset;
                float startPosX = GridManager.instance.gridList[IndexForGrid].startPosX;
                //if it is odd
                if (j % 2 == 1)
                {
                    yPos += GridManager.instance.gridList[IndexForGrid].yOffset;
                }

                GameObject randomPiece = Instantiate(GetRandomPiece(), Vector3.zero, Quaternion.identity);
                GridManager.instance.gridList[IndexForGrid].m_allHexagons[j, i] = randomPiece.GetComponent<HexagonHolder>();
                try
                {
                    GridManager.instance.gridList[IndexForGrid].m_allHexagons[j, i].SetCoord(j, i);
                }
                catch (NullReferenceException ex)
                {
                    Debug.Log("Something went wrong with Hexagon array" + ex.ToString());
                }
                if (randomPiece != null)
                {
                    PlaceGamePiece(randomPiece.GetComponent<HexagonHolder>(), j * xOffset + startPosX, yPos, j, i);
                }
            }
        }
    }

    // UserInputManager class will call this function when there is an input to set clicked cell
    public void SetClickedCell(GameObject cell, Grid grid, Vector2 mouseHitPos)
    {
        if (!alreadyRotating)
        {
            // Set clicked cell
            grid.m_clickedCell = cell.GetComponent<Cell>();
            // Set the selection direction relative our user input hit position
            SetSelection(cell.GetComponent<Cell>(), mouseHitPos);
            // Find and set selected hexagon group
            grid.m_selectedHexagonGroup = FindHexagonGroup(grid.m_allHexagons[grid.m_clickedCell.yIndex, grid.m_clickedCell.xIndex], grid);
        }
    }

    /*
     *  Check input hit position relative to cell we hit and set selection
     */
    public void SetSelection(Cell cell, Vector2 mouseHitPos)
    {
        // TOP LEFT
        if (mouseHitPos.x < cell.transform.position.x && mouseHitPos.y > cell.transform.position.y)
        {
            selection = Selection.TOP_LEFT;
        }
        // TOP RIGHT
        else if (mouseHitPos.x > cell.transform.position.x && mouseHitPos.y > cell.transform.position.y)
        {
            selection = Selection.TOP_RIGHT;
        }
        // TODO: Find a better way to check left and right selection - Currently not working - 
        // LEFT
        /*else if (mouseHitPos.x < cell.transform.position.x)
        {
            selection = Selection.LEFT;
        }
        // RIGHT
        else if (mouseHitPos.x > cell.transform.position.x)
        {
            selection = Selection.RIGHT;
        }*/
        // BOTTOM LEFT
        else if (mouseHitPos.x < cell.transform.position.x && mouseHitPos.y < cell.transform.position.y)
        {
            selection = Selection.BOTTOM_LEFT;
        }
        // BOTTOM RIGHT
        else if (mouseHitPos.x > cell.transform.position.x && mouseHitPos.y < cell.transform.position.y)
        {
            selection = Selection.BOTTOM_RIGHT;
        }
    }

    // Finds neighbours of a given hexagon then sets selected hexagon group
    public HexagonHolder[] FindHexagonGroup(HexagonHolder hexagon, Grid grid)
    {
        NeighbourHexagons neighbours = GetNeighbourHexagons(hexagon);
        HexagonHolder[] hexagons = new HexagonHolder[3];
        return SetHexagonGroup(hexagon, grid, hexagons, neighbours);
    }

    // Calls DestroyOutlineShader function for every member of previouslySelectedHexagonGroup array
    public void DestroyOutline()
    {
        try
        {
            GridManager.instance.gridList[0].m_allHexagons[previouslySelectedHexagonGroup[0].xIndex, previouslySelectedHexagonGroup[0].yIndex].DestroyOutlineShader();
            GridManager.instance.gridList[0].m_allHexagons[previouslySelectedHexagonGroup[1].xIndex, previouslySelectedHexagonGroup[1].yIndex].DestroyOutlineShader();
            GridManager.instance.gridList[0].m_allHexagons[previouslySelectedHexagonGroup[2].xIndex, previouslySelectedHexagonGroup[2].yIndex].DestroyOutlineShader();
        }
        catch (NullReferenceException ex)
        {
            Debug.Log(ex.ToString());
        }
        /*if (GridManager.instance.gridList[0].m_allHexagons[previouslySelectedHexagonGroup[0].xIndex, previouslySelectedHexagonGroup[0].yIndex] != null && GridManager.instance.gridList[0].m_allHexagons[previouslySelectedHexagonGroup[1].xIndex, previouslySelectedHexagonGroup[1].yIndex] != null && GridManager.instance.gridList[0].m_allHexagons[previouslySelectedHexagonGroup[2].xIndex, previouslySelectedHexagonGroup[2].yIndex] != null)
        {
            
        */
        
    }

    
    public void CreateOutline(HexagonHolder[] hexagons)
    {
        hexagons[0].MakeOutline();
        hexagons[1].MakeOutline();
        hexagons[2].MakeOutline();
    }

    // Checks every possible group of hexagons for the selected hexagon
    private HexagonHolder[] SetHexagonGroup(HexagonHolder hexagon, Grid grid, HexagonHolder[] hexagons,
        NeighbourHexagons neighbours)
    {
        hexagons[0] = hexagon;
        switch (selection)
        {
            case Selection.TOP_LEFT:
                if (neighbours.top.x < 0 || neighbours.top.x > (grid.width - 1) || neighbours.top.y < 0 ||
                    neighbours.top.y > (grid.height - 1) || neighbours.topLeft.x < 0 ||
                    neighbours.topLeft.x > (grid.width - 1) || neighbours.topLeft.y < 0 ||
                    neighbours.topLeft.y > (grid.height - 1))
                {
                    goto case Selection.TOP_RIGHT;
                }

                hexagons[1] = grid.m_allHexagons[(int) neighbours.topLeft.x, (int) neighbours.topLeft.y];
                hexagons[2] = grid.m_allHexagons[(int) neighbours.top.x, (int) neighbours.top.y];

                DestroyOutline();

                previouslySelectedHexagonGroup[0] = hexagon;
                previouslySelectedHexagonGroup[1] = grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y];
                previouslySelectedHexagonGroup[2] = grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y];

                
                CreateOutline(hexagons);

                break;
            case Selection.TOP_RIGHT:
                if (neighbours.top.x < 0 || neighbours.top.x > (grid.width - 1) || neighbours.top.y < 0 ||
                    neighbours.top.y > (grid.height - 1) || neighbours.topRight.x < 0 ||
                    neighbours.topRight.x > (grid.width - 1) || neighbours.topRight.y < 0 ||
                    neighbours.topRight.y > (grid.height - 1))
                {
                    goto case Selection.LEFT;
                }
                hexagons[1] = grid.m_allHexagons[(int) neighbours.top.x, (int) neighbours.top.y];
                hexagons[2] = grid.m_allHexagons[(int) neighbours.topRight.x, (int) neighbours.topRight.y];

                DestroyOutline();

                previouslySelectedHexagonGroup[0] = hexagon;
                previouslySelectedHexagonGroup[1] = grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y];
                previouslySelectedHexagonGroup[2] = grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y];

                
                CreateOutline(hexagons);
                break;
            case Selection.LEFT:
                if (neighbours.topLeft.x < 0 || neighbours.topLeft.x > (grid.width - 1) || neighbours.topLeft.y < 0 ||
                    neighbours.topLeft.y > (grid.height - 1) || neighbours.bottomLeft.x < 0 ||
                    neighbours.bottomLeft.x > (grid.width - 1) || neighbours.bottomLeft.y < 0 ||
                    neighbours.bottomLeft.y > (grid.height - 1))
                {
                    goto case Selection.RIGHT;
                }

                hexagons[2] = grid.m_allHexagons[(int) neighbours.topLeft.x, (int) neighbours.topLeft.y];
                hexagons[1] = grid.m_allHexagons[(int) neighbours.bottomLeft.x, (int) neighbours.bottomLeft.y];

                DestroyOutline();

                previouslySelectedHexagonGroup[0] = hexagon;
                previouslySelectedHexagonGroup[1] = grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y];
                previouslySelectedHexagonGroup[2] = grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y];

                
                CreateOutline(hexagons);
                break;
            case Selection.RIGHT:
                if (neighbours.topRight.x < 0 || neighbours.topRight.x > (grid.width - 1) || neighbours.topRight.y < 0 ||
                    neighbours.topRight.y > (grid.height - 1) || neighbours.bottomRight.x < 0 ||
                    neighbours.bottomRight.x > (grid.width - 1) || neighbours.bottomRight.y < 0 ||
                    neighbours.bottomRight.y > (grid.height - 1))
                {
                    goto case Selection.BOTTOM_LEFT;
                }

                hexagons[2] = grid.m_allHexagons[(int) neighbours.topRight.x, (int) neighbours.topRight.y];
                hexagons[1] = grid.m_allHexagons[(int) neighbours.bottomRight.x, (int) neighbours.bottomRight.y];

                DestroyOutline();

                previouslySelectedHexagonGroup[0] = hexagon;
                previouslySelectedHexagonGroup[1] = grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y];
                previouslySelectedHexagonGroup[2] = grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y];

                
                CreateOutline(hexagons);
                break;
            case Selection.BOTTOM_LEFT:
                if (neighbours.bottomLeft.x < 0 || neighbours.bottomLeft.x > (grid.width - 1) || neighbours.bottomLeft.y < 0 ||
                    neighbours.bottomLeft.y > (grid.height - 1) || neighbours.bottom.x < 0 ||
                    neighbours.bottom.x > (grid.width - 1) || neighbours.bottom.y < 0 ||
                    neighbours.bottom.y > (grid.height - 1))
                {
                    goto case Selection.BOTTOM_RIGHT;
                }

                hexagons[1] = grid.m_allHexagons[(int) neighbours.bottom.x, (int) neighbours.bottom.y];
                hexagons[2] = grid.m_allHexagons[(int) neighbours.bottomLeft.x, (int) neighbours.bottomLeft.y];

                DestroyOutline();

                previouslySelectedHexagonGroup[0] = hexagon;
                previouslySelectedHexagonGroup[1] = grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y];
                previouslySelectedHexagonGroup[2] = grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y];

                
                CreateOutline(hexagons);
                break;
            case Selection.BOTTOM_RIGHT:
                if (neighbours.bottomRight == null || neighbours.bottom == null || neighbours.bottomRight.x < 0 || neighbours.bottomRight.x > (grid.width - 1) || neighbours.bottomRight.y < 0 ||
                    neighbours.bottomRight.y > (grid.height - 1) || neighbours.bottom.x < 0 ||
                    neighbours.bottom.x > (grid.width - 1) || neighbours.bottom.y < 0 ||
                    neighbours.bottom.y > (grid.height - 1))
                {
                    goto case Selection.TOP_LEFT;
                }

                hexagons[1] = grid.m_allHexagons[(int) neighbours.bottomRight.x, (int) neighbours.bottomRight.y];
                hexagons[2] = grid.m_allHexagons[(int) neighbours.bottom.x, (int) neighbours.bottom.y];

                DestroyOutline();

                previouslySelectedHexagonGroup[0] = hexagon;
                previouslySelectedHexagonGroup[1] = grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y];
                previouslySelectedHexagonGroup[2] = grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y];

                
                CreateOutline(hexagons);
                break;
            default:
                hexagons[1] = null;
                hexagons[2] = null;
                break;
        }


        return hexagons;
    }

    // Checks every possible group for explosion, fills the explosion list
    private HexagonHolder[] CheckExplosionHexagonGroup(HexagonHolder hexagon, Grid grid, HexagonHolder[] hexagons,
        NeighbourHexagons neighbours)
    {
        hexagons[0] = hexagon;
        if (neighbours.top.x < 0 || neighbours.top.x > (grid.width - 1) || neighbours.top.y < 0 ||
            neighbours.top.y > (grid.height - 1) || neighbours.topLeft.x < 0 ||
            neighbours.topLeft.x > (grid.width - 1) || neighbours.topLeft.y < 0 ||
            neighbours.topLeft.y > (grid.height - 1) || grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y].color != grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y].color)
        {
            
        }
        else
        {
            hexagons[1] = grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y];
            hexagons[2] = grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y];
            return hexagons;
        }
                
                
           
        if (neighbours.top.x < 0 || neighbours.top.x > (grid.width - 1) || neighbours.top.y < 0 ||
            neighbours.top.y > (grid.height - 1) || neighbours.topRight.x < 0 ||
            neighbours.topRight.x > (grid.width - 1) || neighbours.topRight.y < 0 ||
            neighbours.topRight.y > (grid.height - 1) || grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y].color != grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y].color)
        {

        }
        else 
        {
            hexagons[1] = grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y];
            hexagons[2] = grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y];
            return hexagons;
        }

            
        if (neighbours.topLeft.x < 0 || neighbours.topLeft.x > (grid.width - 1) || neighbours.topLeft.y < 0 ||
            neighbours.topLeft.y > (grid.height - 1) || neighbours.bottomLeft.x < 0 ||
            neighbours.bottomLeft.x > (grid.width - 1) || neighbours.bottomLeft.y < 0 ||
            neighbours.bottomLeft.y > (grid.height - 1) || grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y].color != grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y].color)
        {
            
        }
        else
        {
            hexagons[1] = grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y];
            hexagons[2] = grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y];
            return hexagons;
        }

        
        if (neighbours.topRight.x < 0 || neighbours.topRight.x > (grid.width - 1) || neighbours.topRight.y < 0 ||
            neighbours.topRight.y > (grid.height - 1) || neighbours.bottomRight.x < 0 ||
            neighbours.bottomRight.x > (grid.width - 1) || neighbours.bottomRight.y < 0 ||
            neighbours.bottomRight.y > (grid.height - 1) || grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y].color != grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y].color)
        {
            
        }
        else
        {
            hexagons[1] = grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y];
            hexagons[2] = grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y];
            return hexagons;
        }
        
        if (neighbours.bottomLeft.x < 0 || neighbours.bottomLeft.x > (grid.width - 1) || neighbours.bottomLeft.y < 0 ||
            neighbours.bottomLeft.y > (grid.height - 1) || neighbours.bottom.x < 0 ||
            neighbours.bottom.x > (grid.width - 1) || neighbours.bottom.y < 0 ||
            neighbours.bottom.y > (grid.height - 1) || grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y].color != grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y].color)
        {
            
        }
        else
        {
            hexagons[1] = grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y];
            hexagons[2] = grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y];
            return hexagons;
        }
        
        if (neighbours.bottomRight.x < 0 || neighbours.bottomRight.x > (grid.width - 1) || neighbours.bottomRight.y < 0 ||
            neighbours.bottomRight.y > (grid.height - 1) || neighbours.bottom.x < 0 ||
            neighbours.bottom.x > (grid.width - 1) || neighbours.bottom.y < 0 ||
            neighbours.bottom.y > (grid.height - 1) || grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y].color != hexagon.color || grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y].color != grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y].color)
        {
            
        }
        else
        {
            hexagons[1] = grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y];
            hexagons[2] = grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y];
            return hexagons;
        }
        hexagons[1] = null;
        hexagons[2] = null;
        
        return hexagons;
    }

    
    public void SetAlreadyRotation()
    {
        alreadyRotating = false;
    }
    
    // Starts rotation coroutine
    public void Rotate(bool clockwise)
    {
        if (!alreadyRotating)
        {
            alreadyRotating = true; 
            StartCoroutine(RotationRoutine(clockwise, SetAlreadyRotation));
            
        }
    }

    /*
     * Rotates the selected hexagon group 3 times
     *
     *  With each rotation it check if there is a match and if there is that match explodes
     *  After explosion this routine also calls hexagonfall and fillafterexplosion function
     *
     */
    private IEnumerator RotationRoutine(bool clockwise, Action SetAlreadyRotating)
    {
        HexagonHolder[] tempGroup = new HexagonHolder[3];
        if (GridManager.instance.gridList[0].m_selectedHexagonGroup[0] != null &&
            GridManager.instance.gridList[0].m_selectedHexagonGroup[1] != null &&
            GridManager.instance.gridList[0].m_selectedHexagonGroup[2] != null)
        {
            tempGroup[0] = GridManager.instance.gridList[0].m_selectedHexagonGroup[0];
            tempGroup[1] = GridManager.instance.gridList[0].m_selectedHexagonGroup[1];
            tempGroup[2] = GridManager.instance.gridList[0].m_selectedHexagonGroup[2];
            // Rotate hexagon group and check game board if there is an explosion(3 color match) break the loop
            for (int i = 0; i < 3; i++)
            {

                SwapHexagonGroup(clockwise);
                SoundManager.instance.TriggerRotateSound();
                yield return new WaitForSeconds(0.3f);

                // Check every group member for possible matches
                // For now we only check for first grid - we will change this function later
                if (CheckMatch(GridManager.instance.gridList[0].m_selectedHexagonGroup[0],
                    GridManager.instance.gridList[0]) || CheckMatch(GridManager.instance.gridList[0].m_selectedHexagonGroup[1],
                    GridManager.instance.gridList[0]) || CheckMatch(GridManager.instance.gridList[0].m_selectedHexagonGroup[2],
                    GridManager.instance.gridList[0]))
                {
                    UIManager.instance.IncreaseMoves();
                    UserInputManager.instance.groupSelected = false;
                    DecreaseBombTimers(GridManager.instance.bombList);
                    MakeHexagonsFall();
                    FillAfterExplosion();
                    DestroyOutline();
                    break;

                }
            }

        }

        SetAlreadyRotating();
    }

    
    
    public void DecreaseBombTimers(List<HexagonHolder> bombList)
    {
        for (int i = 0; i < bombList.Count; i++)
        {
            bombList[i].DecreaseBombTimer();
        }
    }

    // Right now only works for 1 grid
    // Checks every column for null places in all hexagons array
    // for every null place it increases fallcount 
    // finds the coordinates to where to start falling
    // then starts to make every hexagon on that column fall by fallcount times
    public void MakeHexagonsFall()
    {
        int fallcount = 0;
        int fallStartX;
        int fallStartY;
        bool firstEncounter = true;
        // Check every column and get fall count
        for (int i = 0; i < GridManager.instance.gridList[0].width; i++)
        {
            firstEncounter = true;
            fallcount = 0;
            fallStartX = 0;
            fallStartY = 0;
            for (int j = 0; j < GridManager.instance.gridList[0].height; j++)
            {
                if (GridManager.instance.gridList[0].m_allHexagons[i, j] == null)
                {
                    if (firstEncounter)
                    {
                        fallStartX = i;
                        fallStartY = j - 1;
                        firstEncounter = false;
                    }
                    fallcount++;
                }
            }

            // Now start falling
            if (fallcount > 0)
            {
                for (int z = fallStartY; z >= 0; z--)
                {
                    FallSingleHexagon(i, z, i, z + fallcount);
                }
            }
        }
    }

    // Check every column and get should spawn
    // Instantiate hexagon shouldspawn times
    public void FillAfterExplosion()
    {
        List<HexagonHolder> tempList = new List<HexagonHolder>();

        

        int shouldSpawncount = 0;
        // Check every column and get should spawn count
        for (int i = 0; i < GridManager.instance.gridList[0].width; i++)
        {
            shouldSpawncount = 0;
            for (int j = 0; j < GridManager.instance.gridList[0].height; j++)
            {
                if (GridManager.instance.gridList[0].m_allHexagons[i, j] == null)
                {
                    shouldSpawncount++;
                }
            }

            // Now start instantiating
            if (shouldSpawncount > 0)
            {
                for (int z = shouldSpawncount; z > 0; z--)
                {
                    HexagonHolder spawnedHexagon = Instantiate(GetRandomPiece(), new Vector3(i, 10f, 0), Quaternion.identity).GetComponent<HexagonHolder>();
                    spawnedHexagon.Fall(i, z - 1);
                    spawnedHexagon.SetCoord(i, z - 1);
                    GridManager.instance.gridList[0].m_allHexagons[i, z - 1] = spawnedHexagon;
                    tempList.Add(spawnedHexagon);
                }
            }
        }
        if (bombScore - UIManager.instance.score <= 0)
        {
            //Create a bomb
            int bombIndex = Random.Range(0, tempList.Count - 1);
            tempList[bombIndex].isBomb = true;
            GridManager.instance.bombList.Add(tempList[bombIndex]);
            bombScore += 1000;
        }
        // Also checks if spawned hexagons have matches
        // if there is a match explode
        ExplodeStartingMatches(true);

        // Check if there are any possible moves. If not then end the game.
        if (!CheckPossibleMoves())
        {
            Debug.Log("Game End");
            GameFlowManager.instance.SetGameEnd("No possible match available!");
        }

    }

    // Fills the explosion list. if explosion list is not null then it starts exploding
    public bool CheckMatch(HexagonHolder hexagon, Grid grid)
    {
        
        // Get neighbours
        NeighbourHexagons neighbours = GetNeighbourHexagons(hexagon);
        // Check every possible direction (TOP-RIGHT TOP-LEFT etc)
        explosionList = CheckExplosionHexagonGroup(hexagon, grid, explosionList, neighbours);
        // if there is a match
        if (explosionList[1] != null && explosionList[2] != null)
        {
            UIManager.instance.IncreaseScore();
            return ExplodeGroup(explosionList, grid);
        }
        else
        {
            return false;
        }


    }

    // Destroys hexagon gameobjects and clears allhexagons array
    public bool ExplodeGroup(HexagonHolder[] explosionList, Grid grid)
    {
        // Clear hexagons from the grid
        for (int i = 0; i < 3; i++)
        {
            if (explosionList[i].isBomb)
            {
                GridManager.instance.bombList.Remove(explosionList[i]);
            }
            ParticleManager.instance.ShowParticle(new Vector2(explosionList[i].transform.position.x, explosionList[i].transform.position.y), explosionList[i].color);
            SoundManager.instance.TriggerExplosionSound();
            //StartCoroutine(CameraController.instance.CameraShake(0.15f, .4f));
            grid.m_allHexagons[explosionList[i].xIndex, explosionList[i].yIndex] = null;
            Destroy(explosionList[i].gameObject);
        }

        return true;

    }

    //TODO: Change this function to support multiple grids
    // Right now only for first grid
    // Finds the middle point for selected hexagon group
    public Vector2 FindMiddlePoint()
    {
        Vector2 middlePos = Vector2.zero;
        for (int i = 0; i < 3; i++)
        {
            // Gets the sum of x and y world coordinates
            middlePos.x += GridManager.instance.gridList[0].m_selectedHexagonGroup[i].transform.position.x;
            middlePos.y += GridManager.instance.gridList[0].m_selectedHexagonGroup[i].transform.position.y;
        }
        // Finds the middle
        middlePos.x = middlePos.x / 3;
        middlePos.y = middlePos.y / 3;
        return middlePos;
    }

    // Currently works for only the first grid
    // Makes a single hexagon fall by starting its animation and setting its new coordinates
    public void FallSingleHexagon(int x, int y, int newX, int newY)
    {
        /*
        first.Animate(secondHexagonPos, clockwise);
        first.SetCoord(x2, y2);
        GridManager.instance.gridList[0].m_allHexagons[x2, y2] = first;
        */
        HexagonHolder currentHexagon = GridManager.instance.gridList[0].m_allHexagons[x, y];
        currentHexagon.Fall(newX, newY);
        currentHexagon.SetCoord(newX, newY);
        GridManager.instance.gridList[0].m_allHexagons[x, y] = null;
        GridManager.instance.gridList[0].m_allHexagons[newX, newY] = currentHexagon;
    }

    // Sets the new coordinates for rotating group and starts the rotation animation
    public void SwapHexagonGroup(bool clockwise)
    {
        // For now we only rotate for the first grid
        // only rotate if a hexagon is selected
        HexagonHolder[] selectedHexagonGroup = GridManager.instance.gridList[0].m_selectedHexagonGroup;
        if (selectedHexagonGroup[0] != null && selectedHexagonGroup[1] != null && selectedHexagonGroup[2] != null)
        {
            int x1, y1, x2, y2, x3, y3;
            Vector2 firstHexagonPos, secondHexagonPos, thirdHexagonPos;
            HexagonHolder first, second, third;

            first = selectedHexagonGroup[0];
            second = selectedHexagonGroup[1];
            third = selectedHexagonGroup[2];

            x1 = first.xIndex;
            y1 = first.yIndex;
            x2 = second.xIndex;
            y2 = second.yIndex;
            x3 = third.xIndex;
            y3 = third.yIndex;

            firstHexagonPos = first.transform.position;
            secondHexagonPos = second.transform.position;
            thirdHexagonPos = third.transform.position;

            if (clockwise == false)
            {
                first.Animate(secondHexagonPos, clockwise);
                first.SetCoord(x2, y2);
                GridManager.instance.gridList[0].m_allHexagons[x2, y2] = first;

                
                second.Animate(thirdHexagonPos, clockwise);
                second.SetCoord(x3, y3);
                GridManager.instance.gridList[0].m_allHexagons[x3, y3] = second;

                
                third.Animate(firstHexagonPos, clockwise);
                third.SetCoord(x1, y1);
                GridManager.instance.gridList[0].m_allHexagons[x1, y1] = third;
            }
            else
            {
                
                first.Animate(thirdHexagonPos, clockwise);
                first.SetCoord(x3, y3);
                GridManager.instance.gridList[0].m_allHexagons[x3, y3] = first;

               
                second.Animate(firstHexagonPos, clockwise);
                second.SetCoord(x1, y1);
                GridManager.instance.gridList[0].m_allHexagons[x1, y1] = second;

                
                third.Animate(secondHexagonPos, clockwise);
                third.SetCoord(x2, y2);
                GridManager.instance.gridList[0].m_allHexagons[x2, y2] = third;
            }
        }
        else
        {
            return;
        }
        
    }

    // Currently works for only the first grid
    // Checks if there are possible moves and if not game ends
    // TODO: We might want to find a better way to do this
    // This is an expensive function
    /*
     * Basically checks neighbour neighbours of a given hexagon pair
     *  For example if the pair is top and bottom we need to find left and right neighbours. Then we find their neighbours and if there are more than 4 hexagons with the same color that means there are possible moves
     *          purple              purple
     *      |    4,6     |           6,6
     *    -3,6-        -5,6-   red
     *      |    4,7     |           6,7
     *          purple     5,7      yellow
     *     3,7
     */
    public bool CheckPossibleMoves()
    {
        List<List<HexagonPair>> pairList = new List<List<HexagonPair>>();
        // Check every hexagon for possible locations and matches
        for (int i = 0; i < GridManager.instance.gridList[0].width - 1; i++)
        {
            for (int j = 0; j < GridManager.instance.gridList[0].height - 1; j++)
            {
                HexagonHolder currentHexagon = GridManager.instance.gridList[0].m_allHexagons[j, i];
                // To get every possible location we need to get neighbours
                NeighbourHexagons neighbours = GetNeighbourHexagons(currentHexagon);
                // Array of hexagonholders to store possible match
                HexagonHolder[] possibleMatch = new HexagonHolder[3];
                // Find adjacent hexagons with the same color
                pairList.Add(FindPairGroup(currentHexagon));
                // Check if any neighbouring group contains the color we need
                
            }
        }

        int possibleMatches = 0;
        /*foreach (List<HexagonPair> pair in pairList)
        {
            foreach (HexagonPair hexpair in pair)
            {
                
                if (CheckNeighbouringGroupForColor(hexpair))
                {
                    possibleMatches++;
                }
            }
            
        }*/

        for (int i = 0; i < pairList.Count; i++)
        {
            List<HexagonPair> hexpair = pairList[i];
            for (int j = 0; j < hexpair.Count; j++)
            {
                HexagonPair currentHexpair = hexpair[j];
                if (CheckNeighbouringGroupForColor(currentHexpair))
                {
                    possibleMatches++;
                }
            }
        }

        if (possibleMatches > 0)
        {
            return true;
        }

        return false;

    }

    // Finds the same color count for every pair
    public bool CheckNeighbouringGroupForColor(HexagonPair pair)
    {
        int samecolorCount = 0;
        Grid currentGrid = GridManager.instance.gridList[0];
        NeighbourHexagons pairNeighbourHexagons = new NeighbourHexagons();
        NeighbourHexagons secondNeighbourHexagons = new NeighbourHexagons();

        // Top Bottom
        if (pair.first.xIndex > 0 && pair.first.xIndex < currentGrid.width - 1 && pair.first.yIndex > 0 && pair.first.yIndex < currentGrid.height - 1 && pair.first.xIndex == pair.second.xIndex && pair.first.yIndex == pair.second.yIndex - 1)
        {
            pairNeighbourHexagons =
                GetNeighbourHexagons(currentGrid.m_allHexagons[pair.first.xIndex - 1, (pair.first.yIndex % 2 == 0) ? pair.first.yIndex : pair.first.yIndex + 1]);
            secondNeighbourHexagons = GetNeighbourHexagons(currentGrid.m_allHexagons[pair.first.xIndex + 1,
                (pair.first.xIndex % 2 == 0) ? pair.first.yIndex : pair.first.yIndex + 1]);
        }

        // Top right Bottom left
        if (pair.first.xIndex > 0 && pair.first.xIndex < currentGrid.width - 1 && pair.first.yIndex > 0 && pair.first.yIndex < currentGrid.height - 1 && pair.first.xIndex - 1 == pair.second.xIndex && pair.first.yIndex ==
            ((pair.first.xIndex % 2 == 0) ? pair.second.yIndex : pair.second.yIndex + 1))
        {
            pairNeighbourHexagons =
                GetNeighbourHexagons(currentGrid.m_allHexagons[pair.second.xIndex, pair.second.yIndex - 1]);
            secondNeighbourHexagons = GetNeighbourHexagons(currentGrid.m_allHexagons[pair.first.xIndex,
                pair.first.yIndex + 1]);
        }

        // Top left Bottom right

        if (pair.first.xIndex > 0 && pair.first.xIndex < currentGrid.width - 1 && pair.first.yIndex > 0 && pair.first.yIndex < currentGrid.height - 1 && pair.first.xIndex == pair.second.xIndex - 1 && pair.first.yIndex == 
            ((pair.first.xIndex % 2 == 0) ? pair.first.yIndex : pair.first.yIndex + 1))
        {
            pairNeighbourHexagons =
                GetNeighbourHexagons(currentGrid.m_allHexagons[pair.second.xIndex, pair.second.yIndex - 1]);
            secondNeighbourHexagons = GetNeighbourHexagons(currentGrid.m_allHexagons[pair.first.xIndex,
                pair.first.yIndex + 1]);
        }
       

        if (pairNeighbourHexagons.top.x < 0 || pairNeighbourHexagons.top.x > (currentGrid.width - 1) || pairNeighbourHexagons.top.y < 0 ||
            pairNeighbourHexagons.top.y > (currentGrid.height - 1))
        {

        }
        else
        {
            // CHECK COLORS
            if (currentGrid.m_allHexagons[(int)pairNeighbourHexagons.top.x, (int)pairNeighbourHexagons.top.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        // second neighbours
        if (secondNeighbourHexagons.top.x < 0 || secondNeighbourHexagons.top.x > (currentGrid.width - 1) || secondNeighbourHexagons.top.y < 0 ||
            secondNeighbourHexagons.top.y > (currentGrid.height - 1))
        {

        }
        else
        {
            
            if (currentGrid.m_allHexagons[(int)secondNeighbourHexagons.top.x, (int)secondNeighbourHexagons.top.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        if (pairNeighbourHexagons.topLeft.x < 0 || pairNeighbourHexagons.topLeft.x > (currentGrid.width - 1) || pairNeighbourHexagons.topLeft.y < 0 ||
            pairNeighbourHexagons.topLeft.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)pairNeighbourHexagons.topLeft.x, (int)pairNeighbourHexagons.topLeft.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        // second
        if (pairNeighbourHexagons.topLeft.x < 0 || pairNeighbourHexagons.topLeft.x > (currentGrid.width - 1) || pairNeighbourHexagons.topLeft.y < 0 ||
            pairNeighbourHexagons.topLeft.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)secondNeighbourHexagons.topLeft.x, (int)secondNeighbourHexagons.topLeft.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }


        if (pairNeighbourHexagons.bottomLeft.x < 0 || pairNeighbourHexagons.bottomLeft.x > (currentGrid.width - 1) || pairNeighbourHexagons.bottomLeft.y < 0 ||
            pairNeighbourHexagons.bottomLeft.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)pairNeighbourHexagons.bottomLeft.x, (int)pairNeighbourHexagons.bottomLeft.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        // second
        if (secondNeighbourHexagons.bottomLeft.x < 0 || secondNeighbourHexagons.bottomLeft.x > (currentGrid.width - 1) || secondNeighbourHexagons.bottomLeft.y < 0 ||
            secondNeighbourHexagons.bottomLeft.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)secondNeighbourHexagons.bottomLeft.x, (int)secondNeighbourHexagons.bottomLeft.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }


        if (pairNeighbourHexagons.bottom.x < 0 ||
            pairNeighbourHexagons.bottom.x > (currentGrid.width - 1) || pairNeighbourHexagons.bottom.y < 0 ||
            pairNeighbourHexagons.bottom.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)pairNeighbourHexagons.bottom.x, (int)pairNeighbourHexagons.bottom.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        //second 
        if (secondNeighbourHexagons.bottom.x < 0 ||
            secondNeighbourHexagons.bottom.x > (currentGrid.width - 1) || secondNeighbourHexagons.bottom.y < 0 ||
            secondNeighbourHexagons.bottom.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)secondNeighbourHexagons.bottom.x, (int)secondNeighbourHexagons.bottom.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        if (pairNeighbourHexagons.topRight.x < 0 || pairNeighbourHexagons.topRight.x > (currentGrid.width - 1) || pairNeighbourHexagons.topRight.y < 0 ||
            pairNeighbourHexagons.topRight.y > (currentGrid.height - 1))
        {

        }

        else
        {
            if (currentGrid.m_allHexagons[(int)pairNeighbourHexagons.topRight.x, (int)pairNeighbourHexagons.topRight.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        // second
        if (secondNeighbourHexagons.topRight.x < 0 || secondNeighbourHexagons.topRight.x > (currentGrid.width - 1) || secondNeighbourHexagons.topRight.y < 0 ||
            secondNeighbourHexagons.topRight.y > (currentGrid.height - 1))
        {

        }

        else
        {
            if (currentGrid.m_allHexagons[(int)secondNeighbourHexagons.topRight.x, (int)secondNeighbourHexagons.topRight.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        if (pairNeighbourHexagons.bottomRight.x < 0 ||
            pairNeighbourHexagons.bottomRight.x > (currentGrid.width - 1) || pairNeighbourHexagons.bottomRight.y < 0 ||
            pairNeighbourHexagons.bottomRight.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)pairNeighbourHexagons.bottomRight.x, (int)pairNeighbourHexagons.bottomRight.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        //second 
        if (secondNeighbourHexagons.bottomRight.x < 0 ||
            secondNeighbourHexagons.bottomRight.x > (currentGrid.width - 1) || secondNeighbourHexagons.bottomRight.y < 0 ||
            secondNeighbourHexagons.bottomRight.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)secondNeighbourHexagons.bottomRight.x, (int)secondNeighbourHexagons.bottomRight.y].color ==
                pair.first.color)
            {
                samecolorCount++;
            }
        }

        if (samecolorCount > 4)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    // Only works for the first grid right now
    // For every hexagon in the first grid check pairs
    public List<HexagonPair> FindPairGroup(HexagonHolder currentHexagon)
    {
        Grid currentGrid = GridManager.instance.gridList[0];
        List<HexagonPair> allPairs = new List<HexagonPair>();
        NeighbourHexagons neighbours = GetNeighbourHexagons(currentHexagon);

        // BOTTOM
        if (neighbours.bottom.x < 0 || neighbours.bottom.x > (currentGrid.width - 1) || neighbours.bottom.y < 0 ||
            neighbours.bottom.y > (currentGrid.height - 2))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y].color == currentHexagon.color)
            {
                HexagonPair foundHexagonPair;
                foundHexagonPair.first = currentHexagon;
                foundHexagonPair.second = currentGrid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y];
                allPairs.Add(foundHexagonPair);
            }
        }

        // Top Right Bottom Left

        if (neighbours.bottomLeft.x < 0 || neighbours.bottomLeft.x > (currentGrid.width - 1) || neighbours.bottomLeft.y < 0 ||
            neighbours.bottomLeft.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y].color == currentHexagon.color)
            {
                HexagonPair foundHexagonPair;
                foundHexagonPair.first = currentHexagon;
                foundHexagonPair.second = currentGrid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y];
                allPairs.Add(foundHexagonPair);
            }
        }

        // Top Left Bottom Right

        if (neighbours.bottomRight.x < 0 || neighbours.bottomRight.x > (currentGrid.width - 1) || neighbours.bottomRight.y < 0 ||
            neighbours.bottomRight.y > (currentGrid.height - 1))
        {

        }
        else
        {
            if (currentGrid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y].color == currentHexagon.color)
            {
                HexagonPair foundHexagonPair;
                foundHexagonPair.first = currentHexagon;
                foundHexagonPair.second = currentGrid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y];
                allPairs.Add(foundHexagonPair);
            }
        }

        return allPairs;
    }

    // Check every column in the first grid and if there is a match explode
    public void ExplodeStartingMatches(bool shouldAddScore)
    {
        Grid currentGrid = GridManager.instance.gridList[0];
        for (int i = 0; i < currentGrid.width; i++)
        {
            for (int j = 0; j < currentGrid.height - 1; j++)
            {
                if (i == 0)
                {
                    if (currentGrid.m_allHexagons[i, j].color == currentGrid.m_allHexagons[i, j + 1].color)
                    {
                        // check right
                        HexagonHolder tempHexagon = currentGrid.m_allHexagons[i + 1, (i % 2 == 0) ? j : j + 1];
                        if (currentGrid.m_allHexagons[i, j].color == tempHexagon.color)
                        {
                            // Explode
                            HexagonHolder[] explosionList = new HexagonHolder[3];
                            explosionList[0] = (currentGrid.m_allHexagons[i, j]);
                            explosionList[1] = (currentGrid.m_allHexagons[i + 1, (i % 2 == 0) ? j : j + 1]);
                            explosionList[2] = (tempHexagon);
                            if (ExplodeGroup(explosionList, currentGrid))
                            {
                                if (shouldAddScore)
                                {
                                    UIManager.instance.IncreaseScore();
                                }
                                MakeHexagonsFall();
                                FillAfterExplosion();
                            }
                        }
                    }
                }
                if(i == currentGrid.width - 1)
                {
                    if (currentGrid.m_allHexagons[i, j].color == currentGrid.m_allHexagons[i, j + 1].color)
                    {
                        // check left
                        HexagonHolder tempHexagon = currentGrid.m_allHexagons[i - 1, (i % 2 == 0) ? j : j + 1];
                        if (currentGrid.m_allHexagons[i, j].color == tempHexagon.color)
                        {
                            // Explode
                            HexagonHolder[] explosionList = new HexagonHolder[3];
                            explosionList[0] = (currentGrid.m_allHexagons[i, j]);
                            explosionList[1] = (currentGrid.m_allHexagons[i - 1, (i % 2 == 0) ? j : j + 1]);
                            explosionList[2] = (tempHexagon);
                            if (ExplodeGroup(explosionList, currentGrid))
                            {
                                MakeHexagonsFall();
                                FillAfterExplosion();
                            }
                        }
                    }
                }
                if(i > 0 && i < currentGrid.width - 1)
                {
                    if (currentGrid.m_allHexagons[i, j].color == currentGrid.m_allHexagons[i, j + 1].color)
                    {
                        // check both sides
                        HexagonHolder tempHexagon = currentGrid.m_allHexagons[i + 1, (i % 2 == 0) ? j : j + 1];
                        if (currentGrid.m_allHexagons[i, j].color == tempHexagon.color)
                        {
                            // Explode
                            HexagonHolder[] explosionList = new HexagonHolder[3];
                            explosionList[0] = (currentGrid.m_allHexagons[i, j]);
                            explosionList[1] = (currentGrid.m_allHexagons[i + 1, (i % 2 == 0) ? j : j + 1]);
                            explosionList[2] = (tempHexagon);
                            if (ExplodeGroup(explosionList, currentGrid))
                            {
                                MakeHexagonsFall();
                                FillAfterExplosion();
                            }
                        }
                        tempHexagon = currentGrid.m_allHexagons[i - 1, (i % 2 == 0) ? j : j + 1];
                        if (currentGrid.m_allHexagons[i, j].color == tempHexagon.color)
                        {
                            // Explode
                            HexagonHolder[] explosionList = new HexagonHolder[3];
                            explosionList[0] = (currentGrid.m_allHexagons[i, j]);
                            explosionList[1] = (currentGrid.m_allHexagons[i - 1, (i % 2 == 0) ? j : j + 1]);
                            explosionList[2] = (tempHexagon);
                            if (ExplodeGroup(explosionList, currentGrid))
                            {
                                MakeHexagonsFall();
                                FillAfterExplosion();
                            }
                        }
                    }
                }
            }
        }
    }

}
