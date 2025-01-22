using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthyBar : MonoBehaviour
{
    public BattleActor BattleActor;
    int value;
    int max;
    float percentfilled { get { return value/max; } }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
