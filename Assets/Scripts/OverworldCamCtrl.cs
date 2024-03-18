using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldCamCtrl : MonoBehaviour
{
    public Camera Camera;
    public Transform CameraTarget;
    public Vector3 CamDirection;
    public float CamDistance;
    public float CamDesiredDistance;
    public Vector3 CamDesiredDirection;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState=CursorLockMode.Locked;
        Cursor.visible=false;
        CamDesiredDirection = (Camera.transform.position - CameraTarget.position).normalized;
        Camera.transform.position = CameraTarget.position + CamDesiredDirection * CamDesiredDistance;
        Camera.transform.LookAt(CameraTarget.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
