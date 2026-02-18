using System;
using UnityEngine;

public class DungeonBorderAnchor : AnchorPoint<DungeonBorderAnchorObject>
{
    protected DungeonBlock parent;
    public enum AnchorDirection
    {
        None, North, South, East, West
    }
    public AnchorDirection direction;
    public AnchorDirection oppositeDirection { 
        get
        {
            switch (direction) 
            {
                case AnchorDirection.North: return AnchorDirection.South;
                case AnchorDirection.South: return AnchorDirection.North;
                case AnchorDirection.East: return AnchorDirection.West;
                case AnchorDirection.West: return AnchorDirection.East;
                default : return AnchorDirection.None;
            }
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
        Vector3 anchorDirection = transform.position - parent.transform.position;
        AnchorDirection bestDirection = AnchorDirection.None;
        float bestDot = 0;
        float dot = Vector3.Dot(anchorDirection, Vector3.forward);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.North;
            Debug.Log(name + " north'd of parent " + this.parent);
        }
        dot = Vector3.Dot(anchorDirection, Vector3.back);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.South;
            Debug.Log(name + " south'd of parent " + this.parent);
        }
        dot = Vector3.Dot(anchorDirection, Vector3.right);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.East;
            Debug.Log(name + " east'd of parent " + this.parent);
        }
        dot = Vector3.Dot(anchorDirection, Vector3.left);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestDirection = AnchorDirection.West;
            Debug.Log(name + " west'd of parent " + this.parent);
        }
        Debug.Log(name + " bestdirection is " + bestDirection + ", direction is " + direction);
        if (direction == AnchorDirection.None && bestDirection != AnchorDirection.None)
        {
            direction = bestDirection;
            Debug.Log(name + " bestdirection assigned as " + bestDirection);
        }
        anchorObject = GetComponentInChildren<DungeonBorderAnchorObject>();
    }
}
