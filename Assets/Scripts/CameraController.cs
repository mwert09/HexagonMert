using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float borderSize;

    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        InitCamera();
    }

    
    //TODO: We might need to change this later
    // Initialise camera position based on grid positions 
    private void InitCamera()
    {
        // Get width and height values for grids
        float width = GridManager.instance.GetWidthSum();
        float height = GridManager.instance.GetHeightSum();
       
        // Change camera position based on width and height
        mainCamera.transform.position = new Vector3( (float)(width -1) / 2f,
             -(float)(height + borderSize) / 2f, -10f);

        //float aspectRatio = (float) width / (float) height;
        //float verticalSize = height + borderSize;
        // float horizontalSize = width + borderSize;
        float verticalSize = height;
        float horizontalSize = width;

        mainCamera.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }
}
