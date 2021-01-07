﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridUtils : MonoBehaviour
{
    public static GridUtils instance;

    public bool alreadyRotating = false;
    
    private int bombScore = 1000;

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

    public struct NeighbourHexagons
    {
        public Vector2 top;
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottom;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }

    HexagonHolder[] explosionList = new HexagonHolder[3];

    private void Awake()
    {
        instance = this;
    }

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

    public void PlaceGamePiece(HexagonHolder hexagon, float xPos, float yPos, int x, int y)
    {
        if (hexagon == null)
        {
            return;
        }

        hexagon.transform.position = new Vector3(xPos, yPos, 0);
        hexagon.transform.rotation = Quaternion.identity;
        hexagon.SetCoord(x,y);
    }

    public void FillRandom(int IndexForGrid)
    {
        int gridWidth = GridManager.instance.gridList[IndexForGrid].width;
        int gridHeight = GridManager.instance.gridList[IndexForGrid].height;

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

    public void SetClickedCell(GameObject cell, Grid grid, Vector2 mouseHitPos)
    {
        if (!alreadyRotating)
        {
            grid.m_clickedCell = cell.GetComponent<Cell>();
            SetSelection(cell.GetComponent<Cell>(), mouseHitPos);

            grid.m_selectedHexagonGroup = FindHexagonGroup(grid.m_allHexagons[grid.m_clickedCell.yIndex, grid.m_clickedCell.xIndex], grid);
        }
    }

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

    public HexagonHolder[] FindHexagonGroup(HexagonHolder hexagon, Grid grid)
    {
        NeighbourHexagons neighbours = GetNeighbourHexagons(hexagon);
        HexagonHolder[] hexagons = new HexagonHolder[3];
        return SetHexagonGroup(hexagon, grid, hexagons, neighbours);
    }

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
                break;
            default:
                hexagons[1] = null;
                hexagons[2] = null;
                break;
        }


        return hexagons;
    }

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

    public void Rotate(bool clockwise)
    {
        if (!alreadyRotating)
        {
            alreadyRotating = true; 
            StartCoroutine(RotationRoutine(clockwise));
        }
    }

    private IEnumerator RotationRoutine(bool clockwise)
    {
        HexagonHolder[] tempGroup = new HexagonHolder[3];
        tempGroup[0] = GridManager.instance.gridList[0].m_selectedHexagonGroup[0];
        tempGroup[1] = GridManager.instance.gridList[0].m_selectedHexagonGroup[1];
        tempGroup[2] = GridManager.instance.gridList[0].m_selectedHexagonGroup[2];
        // Rotate hexagon group and check game board if there is an explosion(3 color match) break the loop
        for (int i = 0; i < 3; i++)
        {
            
            SwapHexagonGroup(clockwise);
            yield return new WaitForSeconds(0.3f);

            // Check every group member for possible matches
            // For now we only check for first grid - we will change this function later
            if (CheckMatch(GridManager.instance.gridList[0].m_selectedHexagonGroup[0],
                    GridManager.instance.gridList[0]) || CheckMatch(GridManager.instance.gridList[0].m_selectedHexagonGroup[1],
                GridManager.instance.gridList[0]) || CheckMatch(GridManager.instance.gridList[0].m_selectedHexagonGroup[2],
                GridManager.instance.gridList[0]))
            {
                UIManager.instance.IncreaseMoves();
                DecreaseBombTimers(GridManager.instance.bombList);
               MakeHexagonsFall();
                break;
                   
            }
        }

        FillAfterExplosion();
        alreadyRotating = false;

    }

    public void DecreaseBombTimers(List<HexagonHolder> bombList)
    {
        for (int i = 0; i < bombList.Count; i++)
        {
            bombList[i].DecreaseBombTimer();
        }
    }

    // Right now only works for 1 grid
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

    // We can find a better approach for this
    public void FillAfterExplosion()
    {
        List<HexagonHolder> tempList = new List<HexagonHolder>();

        

        int shouldSpawncount = 0;
        // Check every column and get fall count
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
            int bombIndex = Random.Range(0, tempList.Count);
            tempList[bombIndex].isBomb = true;
            GridManager.instance.bombList.Add(tempList[bombIndex]);
            bombScore += 1000;
        }
    }

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

    public bool ExplodeGroup(HexagonHolder[] explosionList, Grid grid)
    {
        // Clear hexagons from the grid
        for (int i = 0; i < 3; i++)
        {
            grid.m_allHexagons[explosionList[i].xIndex, explosionList[i].yIndex] = null;
            Destroy(explosionList[i].gameObject);
        }

        return true;

    }

    //TODO: Change this function to support multiple grids
    // Right now only for first grid
    public Vector2 FindMiddlePoint()
    {
        Vector2 middlePos = Vector2.zero;
        for (int i = 0; i < 3; i++)
        {
            middlePos.x += GridManager.instance.gridList[0].m_selectedHexagonGroup[i].transform.position.x;
            middlePos.y += GridManager.instance.gridList[0].m_selectedHexagonGroup[i].transform.position.y;
        }

        middlePos.x = middlePos.x / 3;
        middlePos.y = middlePos.y / 3;
        return middlePos;
    }

    // Currently works for only the first grid
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

   

    

}