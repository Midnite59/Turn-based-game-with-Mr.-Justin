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
    public float CastRadius;
    public float minZoom;
    public float maxZoom;
    public float zoomInterpSpeed;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.lockState=CursorLockMode.Locked;
        //Cursor.visible=false;
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
        //float Scroll2 = -Input.GetAxis("Mouse ScrollWheel");
        float Scroll = Input.mouseScrollDelta.y == 0? 0 : -Mathf.Sign(Input.mouseScrollDelta.y);
        //Debug.Log(Scroll);
        CamDesiredDistance = Mathf.Clamp(CamDesiredDistance + Scroll * zoomInterpSpeed, minZoom, maxZoom);
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
        RaycastHit hit;
        if (Physics.SphereCast(CameraTarget.position, CastRadius, CamDesiredDirection, out hit, CamDesiredDistance))
        {
            CamDistance = hit.distance;
        }
        else
        {
            CamDistance = CamDesiredDistance;
        }
        Camera.transform.position = CameraTarget.position + CamDesiredDirection * CamDistance;
        Camera.transform.LookAt(CameraTarget.position);
    }

    

}
