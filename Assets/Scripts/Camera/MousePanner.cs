using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePanner : MonoBehaviour
{
    [SerializeField]
    public static (float x, float y, float z) defaultCameraPos = (0, 0, -10);


    // the speed of camera movement
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

    // returns the current camera pos for serialization
    public (float x, float y, float z) GetCurPos()
    {
        return (transform.position.x, transform.position.y, transform.position.z);
    }

    // sets camera pos

    public void SetCurPos((float x, float y, float z) pos)
    {
        transform.position = new Vector3(pos.x, pos.y, pos.z);
    }
}
