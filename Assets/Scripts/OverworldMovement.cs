using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OverworldMovement : MonoBehaviour
{
    public Vector3 footPos { get {  return transform.position + Vector3.down; } }
    public float speed;
    public Transform cameraPos;
    public Rigidbody rb;
    public new CapsuleCollider collider;
    public float floorCastDistance;
    public float radius { get { return collider.radius /* * transform.localScale.magnitude */; } }
    Vector3 lastNormal = Vector3.up;
    public float castOffset;
    public float steepSlope = 75;
    public float fallCastDistance = 1f;

    public Vector3 castStart { get { return footPos + (Vector3.up * (radius + castOffset)); } }

    bool snap;
    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(LateFixedUpdate());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movement = Vector3.zero;
        movement += Input.GetAxis("Vertical") * Vector3.Cross(cameraPos.right, lastNormal).normalized;
        movement += Input.GetAxis("Horizontal") * Vector3.Cross(lastNormal, Vector3.ProjectOnPlane(cameraPos.forward, Vector3.up)).normalized;
        movement = movement.normalized;
        //transform.Translate(movement*Time.fixedDeltaTime);
        RaycastHit hitInfo;
        rb.linearVelocity = movement * speed;
        if (!Physics.Raycast(castStart + movement * radius, Vector3.down, out hitInfo, fallCastDistance)) 
        {
            rb.linearVelocity = Vector3.zero;
        }
        Debug.DrawRay(castStart + movement * radius, Vector3.down * fallCastDistance, Color.orange, 1);
        Debug.DrawRay(castStart, cameraPos.forward, Color.green, 1);
        Debug.DrawRay(castStart, Vector3.ProjectOnPlane(cameraPos.forward, Vector3.up).normalized, Color.blue, 1);
        Debug.DrawRay(castStart, Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(cameraPos.forward, Vector3.up), lastNormal).normalized, Color.red, 1);
        Debug.DrawRay(castStart, lastNormal.normalized, Color.white, 1);
        //Debug.DrawRay(castStart, Vector3.Cross(cameraPos.forward, lastNormal).normalized, Color.cyan, 1);
        Debug.DrawRay(castStart, Vector3.Cross(cameraPos.right, lastNormal).normalized, Color.magenta, 1);
        Debug.DrawRay(castStart, Vector3.Cross(lastNormal,Vector3.ProjectOnPlane(cameraPos.forward, Vector3.up)).normalized, Color.yellow, 1);
        snap = true;
    }
    IEnumerator LateFixedUpdate()
    {
        while (isActiveAndEnabled) 
        {
            yield return new WaitForFixedUpdate();

            RaycastHit hitInfo;
            if (snap && Physics.SphereCast(castStart, radius, Vector3.down, out hitInfo, floorCastDistance))
            {
                if (Vector3.Dot(Vector3.up, hitInfo.normal) >= Mathf.Cos(steepSlope * Mathf.Deg2Rad))
                {
                    if (hitInfo.distance > 0.1f)
                    {
                        Debug.DrawLine(castStart, hitInfo.point, Color.red, 0.1f);
                        Debug.DrawRay(castStart, hitInfo.distance * Vector3.down, Color.blue, 0.1f);
                        Debug.DrawRay(castStart, radius * Vector3.down, Color.green, 0.1f);
                        Debug.DrawRay(castStart, radius * -lastNormal, Color.green, 0.1f);
                        lastNormal = hitInfo.normal;
                        //Debug.Break();
                    }
                    transform.position += hitInfo.distance * Vector3.down;
                }
            }
            // Code here
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (Vector3.Dot(Vector3.up, collision.contacts[0].normal) >= Mathf.Cos(steepSlope * Mathf.Deg2Rad))
        {
            lastNormal = collision.contacts[0].normal;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        snap = false;
    }
}
