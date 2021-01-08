using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class UserInputManager : MonoBehaviour
{
    public static UserInputManager instance;

    private bool tap, swipeLeft, swipeRight, swipeUp, swipeDown;
    public bool groupSelected = false;
    private bool tapRequested;
    private bool isDragging = false;
    private Vector2 startTouch, swipeDelta;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        tap = swipeLeft = swipeRight = swipeUp = swipeDown = false;

        #region Standalone Inputs
        if (Input.GetMouseButtonDown(0))
        {
            tapRequested = true;
            isDragging = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (tapRequested) { tap = true; }
            isDragging = false;
            if (!GridUtils.instance.alreadyRotating)
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    GameObject ourHitHexagon = hit.collider.transform.gameObject;
                    GridUtils.instance.SetClickedCell(ourHitHexagon, ourHitHexagon.GetComponentInParent<Grid>(), GetMouseWorldPosition());
                    groupSelected = true;
                }
            }
            Reset();
        }
        #endregion

        #region Mobile Inputs
        if (Input.touches.Length > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                tapRequested = true;
                isDragging = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                if (tapRequested) { tap = true; }
                isDragging = false;
                if (!GridUtils.instance.alreadyRotating)
                {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                    if (hit.collider != null)
                    {
                        GameObject ourHitHexagon = hit.collider.transform.gameObject;
                        GridUtils.instance.SetClickedCell(ourHitHexagon, ourHitHexagon.GetComponentInParent<Grid>(), GetMouseWorldPosition());
                        groupSelected = true;
                    }
                }
                Reset();
            }
        }
        #endregion

        //Calculate the distance
        swipeDelta = Vector2.zero;
        if (isDragging)
        {
            if (Input.touchCount > 0) { swipeDelta = Input.touches[0].position - startTouch; }
            else if (Input.GetMouseButton(0)) { swipeDelta = (Vector2)Input.mousePosition - startTouch; }
        }

        //Did we cross the dead zone?
        if (swipeDelta.magnitude > 50)
        {
            tapRequested = false;
            //Which direction are we swiping?
            float x = swipeDelta.x;
            float y = swipeDelta.y;
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                //Left or right?
                if (x > 0) { swipeRight = true; }
                else { swipeLeft = true; }
                //x > 0 ? swipeRight = true : swipeLeft = true;
            }
            else
            {
                //Up or down?
                if (y > 0) { swipeUp = true; }
                else { swipeDown = true; }
                // y > 0 ? swipeUp = true : swipeDown = true;
            }

            if (swipeLeft && isDragging && groupSelected)
            {
                if (!GridUtils.instance.alreadyRotating && groupSelected)
                {
                    Debug.Log("Left Rotate");
                    GridUtils.instance.Rotate(false);
                }
            }

            if (swipeRight && isDragging && groupSelected)
            {
                if (!GridUtils.instance.alreadyRotating && groupSelected)
                {
                    Debug.Log("Right Rotate");
                    GridUtils.instance.Rotate(true);
                }
            }
            Reset();
        }
    }

    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
        isDragging = false;
        tapRequested = false;
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return mouseWorldPosition;
    }

    public Vector2 SwipeDelta { get { return swipeDelta; } }
    public bool SwipeLeft { get { return swipeLeft; } }
    public bool SwipeRight { get { return swipeRight; } }
    public bool SwipeUp { get { return swipeUp; } }
    public bool SwipeDown { get { return swipeDown; } }
    public bool Tap { get { return tap; } }
}

// We want to use raycasting here because we want to get the exact mouse hit position

    /*// If touch time is longer than maxswipetime, it is not a swipe
    public const float maxSwipeTime = 0.5f;

    // minimum swipe distance 
    public const float minSwipeDistance = 0.17f;

    public static bool swipeClockwise = false;
    public static bool swipeCounterClockwise = false;

    private Vector2 startPos;
    private float startTime;

    public bool groupSelected = false;
    public bool tapRequest;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        swipeClockwise = false;
        swipeCounterClockwise = false;
        tapRequest = false;

        if (!GameFlowManager.instance.paused && !GridUtils.instance.alreadyRotating)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                tapRequest = true;
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    GameObject ourHitHexagon = hit.collider.transform.gameObject;
                    GridUtils.instance.SetClickedCell(ourHitHexagon, ourHitHexagon.GetComponentInParent<Grid>(), GetMouseWorldPosition());
                    groupSelected = true;
                }
            }

            if (Input.touches.Length > 0 && groupSelected) 
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

                    
                    if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                    {
                        if (swipe.x > 0)
                        {
                            /*swipeCounterClockwise = false;
                            swipeClockwise = true
                            GridUtils.instance.Rotate(true);
                        }
                        else
                        {
                            /*swipeClockwise = false;
                            swipeCounterClockwise = true;
                            GridUtils.instance.Rotate(false);
                        }
                    }
                }
            }
        }

    }

    
}*/
