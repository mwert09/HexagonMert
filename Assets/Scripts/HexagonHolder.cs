using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonHolder : MonoBehaviour
{
    public HexagonPieceSO hexagon;
    public int xIndex;
    public int yIndex;
    public int rotationSpeed;

    private bool animate = false;
    private Vector2 newPosToAnimate;
    private Vector2 middle;
    private bool clockwise;

    private bool is_falling = false;
    private float fallXCoord;
    private float fallYCoord;

    private void Awake()
    {
        GetComponent<SpriteRenderer>().color = hexagon.color.color;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (animate)
        {
            /*float newPositionX = Mathf.Lerp(transform.position.x, newPos.x, Time.deltaTime * 1f);
            float newPositionY = Mathf.Lerp(transform.position.y, newPos.y, Time.deltaTime * 1f);
            transform.position = new Vector2(newPositionX, newPositionY);*/

            // Get the middle point to rotate around

            if (clockwise)
            {
                transform.RotateAround(middle, new Vector3(0, 0, 1), Time.deltaTime * 120.0f * rotationSpeed);
            }
            else
            {
                transform.RotateAround(middle, new Vector3(0, 0, 1), Time.deltaTime * -120.0f * rotationSpeed);
            }
            
            if (Vector3.Distance(transform.position, newPosToAnimate) < 0.1f)
            {
                transform.position = newPosToAnimate;
                transform.rotation  = Quaternion.Euler(0,0,0);
                animate = false;
            }
        }

        if (is_falling)
        {
            transform.position = new Vector3(fallXCoord, Mathf.Lerp(transform.position.y, fallYCoord, 0.2f), 0f);
            if (Vector3.Distance(transform.position, new Vector3(fallXCoord, fallYCoord, 0f)) < 0.1f)
            {
                transform.position = new Vector3(fallXCoord, fallYCoord, 0f);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                is_falling = false;
            }
        }
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Animate(Vector2 newPos, bool clockwiseb)
    {
        newPosToAnimate = newPos;
        clockwise = clockwiseb;
        middle = GridUtils.instance.FindMiddlePoint();
        animate = true;
    }

    public void Fall(int newX, int newY)
    {
        fallXCoord = GridManager.instance.gridList[0].m_allCells[newX, newY].transform.position.x;
        fallYCoord = GridManager.instance.gridList[0].m_allCells[newX, newY].transform.position.y;
        is_falling = true;
    }

    
}
