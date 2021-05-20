using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePanner : MonoBehaviour
{
    [SerializeField]
    public static (float x, float y, float z) defaultCameraPos = (0, 0, -10);

    public float dragSpeed = 2f;
    private Vector3 originMousePos;
    private Vector3 originCameraWorldPos;
    
    // Update is called once per frame
    void Update()
    {
        // clicking the button
        if (Input.GetMouseButtonDown(1))
        {
            originMousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            originCameraWorldPos = transform.position;
        }
        // has the button held
        if (Input.GetMouseButton(1))
        {
            Vector3 curPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector3 deltaPos = curPos - originMousePos;
            transform.position = originCameraWorldPos - deltaPos * dragSpeed;
        }
    }

    public (float x, float y, float z) GetCurPos()
    {
        return (transform.position.x, transform.position.y, transform.position.z);
    }

    public void SetCurPos((float x, float y, float z) pos)
    {
        transform.position = new Vector3(pos.x, pos.y, pos.z);
    }
}
