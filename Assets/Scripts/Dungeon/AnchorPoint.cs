using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using System.Linq;

public class AnchorPoint<T> : MonoBehaviour where T : AnchorObject 
{
    public T anchorObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        anchorObject = GetComponentInChildren<T>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
