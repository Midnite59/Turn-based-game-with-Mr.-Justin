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
    private float CamYaw;
    private float CamPitch;
    public float CamYawMultiplier;
    public float CamPitchMultiplier;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState=CursorLockMode.Locked;
        Cursor.visible=false;
        CamDesiredDirection = (Camera.transform.position - CameraTarget.position).normalized;
        Camera.transform.position = CameraTarget.position + CamDesiredDirection * CamDesiredDistance;
        Camera.transform.LookAt(CameraTarget.position);
        Vector3 CamEuler = Camera.transform.rotation.eulerAngles;
        CamPitch = CamEuler.x;
        CamYaw = CamEuler.y;
    }

    // Update is called once per frame
    void Update()
    {
        float InputY = Input.GetAxis("Mouse X");
        float InputX = -Input.GetAxis("Mouse Y");
        CamYaw += InputY * CamYawMultiplier;
        CamPitch = Mathf.Clamp(CamPitch + InputX * CamPitchMultiplier, -89, 89);
        if (CamYaw > 180)
        {
            CamYaw -= 360;
        }
        if (CamYaw < -180)
        {
            CamYaw += 360;
        }
        Quaternion CamNewEuler = Quaternion.Euler(CamPitch, CamYaw, 0);
        CamDesiredDirection = CamNewEuler * -Vector3.forward;
        Camera.transform.position = CameraTarget.position + CamDesiredDirection * CamDesiredDistance;
        Camera.transform.LookAt(CameraTarget.position);
    }
}
