using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class UserInputManager : MonoBehaviour
{

    // We want to use raycasting here because we want to get the exact mouse hit position

    // If touch time is longer than maxswipetime, it is not a swipe
    public const float maxSwipeTime = 0.5f;

    // minimum swipe distance 
    public const float minSwipeDistance = 0.17f;

    public static bool swipeClockwise = false;
    public static bool swipeCounterClockwise = false;

    private Vector2 startPos;
    private float startTime;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        swipeClockwise = false;
        swipeCounterClockwise = false;

        if (!GameFlowManager.instance.paused)
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

            if (Input.touches.Length > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    startPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);
                    startTime = Time.time;
                }

                if (t.phase == TouchPhase.Ended)
                {
                    // Pressed too long
                    if (Time.time - startTime > maxSwipeTime)
                        return;
                    Vector2 endPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);

                    Vector2 swipe = new Vector2(endPos.x - startPos.x, endPos.y - startPos.y);

                    // Too short
                    if (swipe.magnitude < minSwipeDistance)
                        return;

                    //TODO: FIX
                    if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                    {
                        if (swipe.x > 0)
                        {
                            /*swipeCounterClockwise = false;
                            swipeClockwise = true*/
                            GridUtils.instance.Rotate(true);
                        }
                        else
                        {
                            /*swipeClockwise = false;
                            swipeCounterClockwise = true;*/
                            GridUtils.instance.Rotate(false);
                        }
                    }
                }
            }
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return mouseWorldPosition;
    }
}
