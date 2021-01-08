using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/* Basic camera controller - It basically changes x,y positions of our main camera and sets orthographic size to make grid fit on screen */
public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    // padding
    public float borderSize;

    private Camera mainCamera;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        InitCamera();
    }

    
    //TODO: We might want to change this later
    // Initialise camera position based on grid positions 
    private void InitCamera()
    {
        try
        {
            // Get grid width and height sum values
            float width = GridManager.instance.GetWidthSum();
            float height = GridManager.instance.GetHeightSum();

            // Change camera position based on width and height
            mainCamera.transform.position = new Vector3((float)(width - 1) / 2f,
                -(float)(height + borderSize) / 2f, -10f);

            //float aspectRatio = (float) width / (float) height;
            //float verticalSize = height + borderSize;
            // float horizontalSize = width + borderSize;
            float verticalSize = height;
            float horizontalSize = width;

            mainCamera.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
        }
        catch (NullReferenceException ex)
        {
            Debug.Log(ex.ToString());
        }
        
    }

    /*public IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.position;

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, y, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
    }*/
}
