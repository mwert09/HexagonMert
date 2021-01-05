using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class UserInputManager : MonoBehaviour
{

    // We want to use raycasting here because we want to get the exact mouse hit position
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                GameObject ourHitHexagon = hit.collider.transform.gameObject;
                GridUtils.instance.SetClickedCell(ourHitHexagon, ourHitHexagon.GetComponentInParent<Grid>(), GetMouseWorldPosition());
            }
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return mouseWorldPosition;
    }
}
