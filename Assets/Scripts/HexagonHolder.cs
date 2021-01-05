using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonHolder : MonoBehaviour
{
    public HexagonPieceSO hexagon;
    public int xIndex;
    public int yIndex;

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
        
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }
}
