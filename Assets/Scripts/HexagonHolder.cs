using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/* Hexagon game piece */
public class HexagonHolder : MonoBehaviour
{
    /*
     *
     *
     * I was going to make this with Scriptable objects but i changed my mind
     * But i didn't delete everything so i can go back to it if i want to
     *
     *
     */
    //public HexagonPieceSO hexagon;
    [Header("Hexagon")] 
    public Sprite defaultSprite;
    public int xIndex;
    public int yIndex;
    public Color color;
    [Range(5.0f, 50.0f) ] public int rotationSpeed;

    /*Variables for rotate animation*/
    private bool animate = false;
    private Vector2 newPosToAnimate;
    private Vector2 middle;
    private bool clockwise;

    /*Variables for fall animation*/
    private bool is_falling = false;
    private float fallXCoord;
    private float fallYCoord;

    /*Variables for shader*/
    [Header("Shader")]
    public Material OutlineMaterial;
    public Material DefaultMaterial;


    [Header("Bomb Part")]
    public bool isBomb;
    public Sprite bombSprite;
    public int movesBeforeExplosion;
    public GameObject bombText;


    private void Awake()
    {
        // Set color of hexagons
        GetComponent<SpriteRenderer>().color = color;
    }

    // Start is called before the first frame update
    void Start()
    {
        // If this hexagon is a bomb then change it's sprite and start showing bomb countdown text
        if (isBomb)
        {
            GetComponent<SpriteRenderer>().sprite = bombSprite;
            bombText.SetActive(true);
        }
        else
        {
            bombText.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (animate)
        {
            // Get the middle point to rotate around
            if (clockwise)
            {
                transform.RotateAround(middle, new Vector3(0, 0, 1), Time.deltaTime * 120.0f * rotationSpeed);
            }
            if(!clockwise)
            {
                transform.RotateAround(middle, new Vector3(0, 0, 1), Time.deltaTime * -120.0f * rotationSpeed);
            }
            // If hexagon is close to destination set it's new x,y coordinates and also reset any rotation applied before
            if (Vector3.Distance(transform.position, newPosToAnimate) < 0.1f)
            {
                transform.position = newPosToAnimate;
                transform.rotation  = Quaternion.Euler(0,0,0);
                animate = false;
            }
        }

        if (is_falling)
        {
            // start falling
            transform.position = new Vector3(fallXCoord, Mathf.Lerp(transform.position.y, fallYCoord, 0.2f), 0f);
            // If hexagon is close to destination set it's new x,y coordinates and also reset any rotation applied before
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

    /*Decreases bomb countdown by 1 also checks if it is exploded or not*/
    public void DecreaseBombTimer()
    {
        movesBeforeExplosion--;
        bombText.GetComponent<TextMesh>().text = movesBeforeExplosion.ToString();
        if (movesBeforeExplosion == 0)
        {
            GameFlowManager.instance.SetGameEnd("BOOOOM!");
        }
    }

    // Changes sprite material to make a little glow around selected hexagon group
    public void MakeOutline()
    {
        gameObject.GetComponent<SpriteRenderer>().material = OutlineMaterial;
        
    }

    // Changes sprite material to default 
    public void DestroyOutlineShader()
    {
        gameObject.GetComponent<SpriteRenderer>().material = DefaultMaterial;
        
    }

    public void SetBombSprite()
    {
        GetComponent<SpriteRenderer>().sprite = bombSprite;
    }

    public void SetDefaultSprite()
    {
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
    }
    
}
