using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CamaraControl : MonoBehaviour
{
    public Camera Cam;
    public float ZoomStep = 1;
    public float MinCamSize = 1.0f;
    public float MaxCamSize = 20.0f;
    public float SmoothSpeed = 15.0f;
    public float mapMinX = -50.0f;
    public float mapMaxX = 50.0f;
    public float mapMinY = -50.0f;
    public float mapMaxY = 50.0f;

    private Vector3 dragOrigin;
    private float targetOrtho = 13.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PanCamera();
        Zoom();
    }

    void PanCamera()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(!EventSystem.current.IsPointerOverGameObject()) //UI 클릭시 false
                {
                    dragOrigin = Cam.ScreenToWorldPoint(Input.mousePosition);
                }
        }

        if(Input.GetMouseButton(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 difference = dragOrigin - Cam.ScreenToWorldPoint(Input.mousePosition);
                Cam.transform.position += difference;
            }
        }

        ClampCamera(Cam.transform.position);
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f)
        {
            targetOrtho -= scroll * ZoomStep;
            targetOrtho = Mathf.Clamp(targetOrtho, MinCamSize, MaxCamSize);
        }

        Cam.orthographicSize = Mathf.MoveTowards(Cam.orthographicSize, targetOrtho, SmoothSpeed * Time.deltaTime);

        ClampCamera(Cam.transform.position);
    }

    void ClampCamera(Vector3 targetposition)
    {
        float camHeight = Cam.orthographicSize;
        float camWidth = Cam.orthographicSize * Cam.aspect;

        float minX = mapMinX + camWidth;
        float minY = mapMinY + camHeight;

        float maxX = mapMaxX - camWidth;
        float maxY = mapMaxY - camHeight;

        float newX = Mathf.Clamp(targetposition.x, minX, maxX);
        float newY = Mathf.Clamp(targetposition.y, minY, maxY);

        Cam.transform.position = new Vector3(newX, newY, -10);
    }
}
