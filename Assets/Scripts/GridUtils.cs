using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridUtils : MonoBehaviour
{
    public static GridUtils instance;

    public bool alreadyRotation = false;

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
        neighbours.topLeft = new Vector2(selectedHexagon.xIndex - 1, (selectedHexagon.xIndex % 2 == 0) ? selectedHexagon.yIndex - 1 : selectedHexagon.yIndex);
        neighbours.topRight = new Vector2(selectedHexagon.xIndex + 1, (selectedHexagon.xIndex % 2 == 0) ? selectedHexagon.yIndex - 1 : selectedHexagon.yIndex);
        neighbours.bottomLeft = new Vector2(selectedHexagon.xIndex - 1, (selectedHexagon.xIndex % 2 == 0) ? selectedHexagon.yIndex : selectedHexagon.yIndex + 1);
        neighbours.bottomRight = new Vector2(selectedHexagon.xIndex + 1, (selectedHexagon.xIndex % 2 == 0) ? selectedHexagon.yIndex : selectedHexagon.yIndex + 1);
        return neighbours;
    }

    public GameObject GetRandomPiece()
    {
        int randomIndex = Random.Range(0, GridManager.instance.hexagonPrefabs.Length);
        if (GridManager.instance.hexagonPrefabs[randomIndex] == null)
        {
            Debug.Log("Grid: " + randomIndex + "does not contain a valid hexagon prefab");
        }

        return GridManager.instance.hexagonPrefabs[randomIndex];
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
        grid.m_clickedCell = cell.GetComponent<Cell>();
        SetSelection(cell.GetComponent<Cell>(), mouseHitPos);

        grid.m_selectedHexagonGroup = FindHexagonGroup(grid.m_allHexagons[grid.m_clickedCell.yIndex, grid.m_clickedCell.xIndex], grid);
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
        bool shouldNotLoop = false;

        do
        {
            hexagons[0] = hexagon;
            switch (selection)
            {
                case Selection.TOP_LEFT:
                    if (neighbours.top.x < 0 || neighbours.top.x > grid.width || neighbours.top.y < 0 ||
                        neighbours.top.y > grid.height || neighbours.topLeft.x < 0 ||
                        neighbours.topLeft.x > grid.width || neighbours.topLeft.y < 0 ||
                        neighbours.topLeft.y > grid.height)
                    {
                        goto case Selection.TOP_RIGHT;
                    }
                    hexagons[1] = grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y];
                    hexagons[2] = grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y];
                    break;
                case Selection.TOP_RIGHT:
                    if (neighbours.top.x < 0 || neighbours.top.x > grid.width || neighbours.top.y < 0 ||
                        neighbours.top.y > grid.height || neighbours.topRight.x < 0 ||
                        neighbours.topRight.x > grid.width || neighbours.topRight.y < 0 ||
                        neighbours.topRight.y > grid.height)
                    {
                        goto case Selection.LEFT;
                    }
                    hexagons[1] = grid.m_allHexagons[(int)neighbours.top.x, (int)neighbours.top.y];
                    hexagons[2] = grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y];
                    break;
                case Selection.LEFT:
                    if (neighbours.topLeft.x < 0 || neighbours.topLeft.x > grid.width || neighbours.topLeft.y < 0 ||
                        neighbours.topLeft.y > grid.height || neighbours.bottomLeft.x < 0 ||
                        neighbours.bottomLeft.x > grid.width || neighbours.bottomLeft.y < 0 ||
                        neighbours.bottomLeft.y > grid.height)
                    {
                        goto case Selection.RIGHT;
                    }
                    hexagons[1] = grid.m_allHexagons[(int)neighbours.topLeft.x, (int)neighbours.topLeft.y];
                    hexagons[2] = grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y];
                    break;
                case Selection.RIGHT:
                    if (neighbours.topRight.x < 0 || neighbours.topRight.x > grid.width || neighbours.topRight.y < 0 ||
                        neighbours.topRight.y > grid.height || neighbours.bottomRight.x < 0 ||
                        neighbours.bottomRight.x > grid.width || neighbours.bottomRight.y < 0 ||
                        neighbours.bottomRight.y > grid.height)
                    {
                        goto case Selection.BOTTOM_LEFT;
                    }
                    hexagons[1] = grid.m_allHexagons[(int)neighbours.topRight.x, (int)neighbours.topRight.y];
                    hexagons[2] = grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y];
                    break;
                case Selection.BOTTOM_LEFT:
                    if (neighbours.bottomLeft.x < 0 || neighbours.bottomLeft.x > grid.width || neighbours.bottomLeft.y < 0 ||
                        neighbours.bottomLeft.y > grid.height || neighbours.bottom.x < 0 ||
                        neighbours.bottom.x > grid.width || neighbours.bottom.y < 0 ||
                        neighbours.bottom.y > grid.height)
                    {
                        goto case Selection.BOTTOM_RIGHT;
                    }
                    hexagons[1] = grid.m_allHexagons[(int)neighbours.bottomLeft.x, (int)neighbours.bottomLeft.y];
                    hexagons[2] = grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y];
                    break;
                case Selection.BOTTOM_RIGHT:
                    if (neighbours.bottomRight.x < 0 || neighbours.bottomRight.x > grid.width || neighbours.bottomRight.y < 0 ||
                        neighbours.bottomRight.y > grid.height || neighbours.bottom.x < 0 ||
                        neighbours.bottom.x > grid.width || neighbours.bottom.y < 0 ||
                        neighbours.bottom.y > grid.height)
                    {
                        goto case Selection.TOP_LEFT;
                    }
                    hexagons[1] = grid.m_allHexagons[(int)neighbours.bottomRight.x, (int)neighbours.bottomRight.y];
                    hexagons[2] = grid.m_allHexagons[(int)neighbours.bottom.x, (int)neighbours.bottom.y];
                    break;
                default:
                    hexagons[1] = null;
                    hexagons[2] = null;
                    break;
            }

            if (hexagons[1].xIndex < 0 || hexagons[1].xIndex > grid.width || hexagons[1].yIndex < 0 || hexagons[1].yIndex > grid.height || hexagons[2].xIndex < 0 || hexagons[2].xIndex > grid.width || hexagons[2].yIndex < 0 || hexagons[2].yIndex > grid.height)
            {
                selection++;

            }
            else
            {
                shouldNotLoop = true;
            }

        } while (!shouldNotLoop);
        
        return hexagons;
    }

    public void Rotate(bool clockwise)
    {
        if (!alreadyRotation)
        {
            alreadyRotation = true; 
            StartCoroutine(RotationRoutine(clockwise));
        }
    }

    private IEnumerator RotationRoutine(bool clockwise)
    {
        // Rotate hexagon group and check game board if there is an explosion(3 color match) break the loop
        for (int i = 0; i < 3; i++)
        {
            SwapHexagonGroup(clockwise);
            Debug.Log("Rutin : " + i);
            yield return new WaitForSeconds(0.7f);

            // Check explosion(3 match)
        }

        alreadyRotation = false;

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

    public void SwapHexagonGroup(bool clockwise)
    {
        // For now we only rotate for the first grid
        // only rotate if a hexagon is selected
        HexagonHolder[] selectedHexagonGroup = GridManager.instance.gridList[0].m_selectedHexagonGroup;
        if (selectedHexagonGroup[0] != null && selectedHexagonGroup[1] != null && selectedHexagonGroup[2] != null)
        {
            Vector2 firstHexagonPos, secondHexagonPos, thirdHexagonPos;
            HexagonHolder first, second, third;

            first = selectedHexagonGroup[0];
            second = selectedHexagonGroup[1];
            third = selectedHexagonGroup[2];

            firstHexagonPos = first.transform.position;
            secondHexagonPos = second.transform.position;
            thirdHexagonPos = third.transform.position;

            if (clockwise == false)
            {
                first.SetCoord(second.xIndex, second.yIndex);
                first.Animate(secondHexagonPos, clockwise);
                GridManager.instance.gridList[0].m_allHexagons[second.xIndex, second.yIndex] = first;

                second.SetCoord(third.xIndex, third.yIndex);
                second.Animate(thirdHexagonPos, clockwise);
                GridManager.instance.gridList[0].m_allHexagons[third.xIndex, third.yIndex] = second;

                third.SetCoord(third.xIndex, third.yIndex);
                third.Animate(firstHexagonPos, clockwise);
                GridManager.instance.gridList[0].m_allHexagons[first.xIndex, first.yIndex] = third;
            }
            else
            {
                first.SetCoord(third.xIndex, third.yIndex);
                first.Animate(thirdHexagonPos, clockwise);
                GridManager.instance.gridList[0].m_allHexagons[third.xIndex, third.yIndex] = first;

                second.SetCoord(first.xIndex, first.yIndex);
                second.Animate(firstHexagonPos, clockwise);
                GridManager.instance.gridList[0].m_allHexagons[first.xIndex, first.yIndex] = second;

                third.SetCoord(second.xIndex, second.yIndex);
                third.Animate(secondHexagonPos, clockwise);
                GridManager.instance.gridList[0].m_allHexagons[second.xIndex, second.yIndex] = third;
            }
        }
        else
        {
            return;
        }
        
    }

    

}
