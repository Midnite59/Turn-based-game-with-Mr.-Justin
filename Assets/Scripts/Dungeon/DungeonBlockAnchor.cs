using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using static DungeonBorderAnchor;

public class DungeonBlockAnchor : AnchorPoint<DungeonBlockAnchorObject>
{
    [Flags]
    public enum PointFlags 
    {
        None = 0, // 0
        Encounter = 1 << 0, // 1 
        Elite = 1 << 1, // 10
        Character = 1 << 2,  // 100
        Treasure = 1 << 3, // 1000
        Shopkeeper = 1 << 4, // ...
        Refresh = 1 << 5,
        SuperRefresh = 1 << 6,
        AnnoyingPuzzle = 1 << 7,
    }

    public PointFlags accTypes;

    public DungeonBlock parent;

    public void SetParent(DungeonBlock parent)
    {
        this.parent = parent;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
