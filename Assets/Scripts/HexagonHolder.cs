using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonHolder : MonoBehaviour
{
    public HexagonPieceSO hexagon;
    public int xIndex;
    public int yIndex;

    private bool animate = false;
    private Vector2 newPosToAnimate;
    private Vector2 middle;
    private bool clockwise;


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
                Debug.Log("clockwise");
                transform.RotateAround(middle, new Vector3(0, 0, 1), Time.deltaTime * 360.0f);
            }
            else
            {
                Debug.Log(" counter clockwise");
                transform.RotateAround(middle, new Vector3(0, 0, 1), Time.deltaTime * -360.0f);
            }
            
            if (Vector3.Distance(transform.position, newPosToAnimate) < 0.1f)
            {
                transform.position = newPosToAnimate;
                transform.rotation  = Quaternion.Euler(0,0,0);
                animate = false;
            }
        }
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Animate(Vector2 newPos, bool clockwise)
    {
        newPosToAnimate = newPos;
        clockwise = this.clockwise;
        middle = GridUtils.instance.FindMiddlePoint();
        animate = true;
    }

    
}
