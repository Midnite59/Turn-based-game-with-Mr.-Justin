using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldMovement : MonoBehaviour
{
    public float speed;
    public Transform cameraPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movement = Vector3.zero;
        movement += Input.GetAxis("Vertical") * Vector3.ProjectOnPlane(cameraPos.forward, Vector3.up).normalized * speed;
        movement += Input.GetAxis("Horizontal") * Vector3.ProjectOnPlane(cameraPos.right, Vector3.up).normalized * speed;
        transform.Translate(movement*Time.fixedDeltaTime);
    }
}
