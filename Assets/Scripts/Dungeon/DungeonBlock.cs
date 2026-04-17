using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using System.Linq;
using AnchorDirection = DungeonBorderAnchor.AnchorDirection;

public class DungeonBlock : MonoBehaviour
{
    public List<DungeonBorderAnchor> anchors;
    public int exits 
    { 
        get 
        { 
            return anchors.Count(a => a.connection == null && !a.anchorObject.isWall); 
        } 
    }
    //public int age;

    [Serializable]
    public class Neighbor 
    {
        public DungeonBlock prefab;
        public AnchorDirection direction;

        public Neighbor(DungeonBlock prefab, AnchorDirection direction) 
        {
            this.prefab = prefab;
            this.direction = direction;
        }

    }

    public List<Neighbor> neighbors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Setup();
    }
    public void Setup()
    {
        anchors.Clear();
        anchors.AddRange(GetComponentsInChildren<DungeonBorderAnchor>().Where(a => !anchors.Contains(a)));
        anchors.ForEach(a => a.SetParent(this));
    }

    public void ConnectAnchors(DungeonBorderAnchor self, DungeonBorderAnchor other) 
    {
        Vector3 diff = self.transform.position - transform.position;
        transform.position = other.transform.position - diff;
        self.Connect(other);
    }
    public bool HasDoor(DungeonBorderAnchor.AnchorDirection direction) 
    {
        return anchors.Any(a => a.direction == direction && a.anchorObject && !a.anchorObject.isWall);
    }
    void DrawConnections()
    {
        foreach (DungeonBorderAnchor anchor in anchors) 
        {
            if (anchor.connection != null) 
            {
                if (!anchor.anchorObject.isWall)
                {
                    Debug.DrawLine(transform.position, anchor.transform.position, anchor.anchorObject.type == anchor.connection.anchorObject.type ? Color.green : Color.red);
                }
            }
            else if (!anchor.anchorObject.isWall)
            {
                Debug.DrawLine(transform.position, anchor.transform.position, Color.yellow);
            }
        }
    }

    public DungeonBorderAnchor GetAnchor(DungeonBorderAnchor.AnchorDirection direction) 
    {
        return anchors.First(a => a.direction == direction);
    }

    public DungeonBorderAnchor GetAnchorOpp(DungeonBorderAnchor.AnchorDirection direction)
    {
        return anchors.First(a => a.oppositeDirection == direction);
    }

    void Update()
    {
        DrawConnections();
    }
}
