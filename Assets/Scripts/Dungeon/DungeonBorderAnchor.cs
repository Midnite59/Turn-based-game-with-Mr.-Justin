using System;
using UnityEngine;

public class DungeonBorderAnchor : AnchorPoint<DungeonBorderAnchorObject>
{
    protected DungeonBlock parent;
    public enum AnchorDirection
    {
        None = 0, 
        North = 3, 
        South = -3, 
        East = 1, 
        West = -1, 
        Up = 9, 
        Down = -9
    }
    public AnchorDirection direction;
    public AnchorDirection oppositeDirection { 
        get
        {
            return (AnchorDirection)(-(int)direction);
        } 
    }
    public AnchorDirection clockwiseDirection
    {
        get
        {
            int x = ((int)direction)%3;
            int y = ((int)direction)/3;

            return (AnchorDirection)(y - (x*3));
        }
    }
    public AnchorDirection counterClockwiseDirection
    {
        get
        {
            int x = ((int)direction) % 3;
            int y = ((int)direction) / 3;

            return (AnchorDirection)(-y + (x * 3));
        }
    }

    public DungeonBorderAnchor connection { get { return _connection; } protected set { _connection = value; } }

    [SerializeField]
    private DungeonBorderAnchor _connection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void Connect(DungeonBorderAnchor other)
    {
        if (connection == null && connection == null)
        {
            connection = other;
            other.connection = this;
        }
        else 
        {
            Debug.LogWarning("Anchors can only connect once!");
        }
    }

    public void SetParent(DungeonBlock parent)
    {
        this.parent = parent;
        Vector3 anchorDirection = transform.position - (parent.transform.position + Vector3.up);
        AnchorDirection bestDirection = AnchorDirection.None;
        float bestDot = 0;
        float dot = Vector3.Dot(anchorDirection, Vector3.forward);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.North;
            //Debug.Log(name + " north'd of parent " + this.parent);
        }
        dot = Vector3.Dot(anchorDirection, Vector3.back);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.South;
            //Debug.Log(name + " south'd of parent " + this.parent);
        }
        dot = Vector3.Dot(anchorDirection, Vector3.right);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.East;
            //Debug.Log(name + " east'd of parent " + this.parent);
        }
        dot = Vector3.Dot(anchorDirection, Vector3.left);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.West;
            //Debug.Log(name + " west'd of parent " + this.parent);
        }
        dot = Vector3.Dot(anchorDirection, Vector3.down);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.Down;
            //Debug.Log(name + " down'd of parent " + this.parent);
        }
        dot = Vector3.Dot(anchorDirection, Vector3.up);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.Up;
            //Debug.Log(name + " up'd of parent " + this.parent);
        }
        
        //Debug.Log(name + " bestdirection is " + bestDirection + ", direction is " + direction);
        if (/*direction == AnchorDirection.None &&*/ bestDirection != AnchorDirection.None)
        {
            direction = bestDirection;
            //Debug.Log(name + " bestdirection assigned as " + bestDirection);
        }
        anchorObject = GetComponentInChildren<DungeonBorderAnchorObject>();
    }

    public void BuildObject(DungeonBorderAnchorObject prefab) 
    {
        if (anchorObject != null) 
        {
            Destroy(anchorObject.gameObject);
            anchorObject = null;
        }
        anchorObject = Instantiate(prefab, transform);
        anchorObject.transform.localPosition = Vector3.zero;
    }
}
