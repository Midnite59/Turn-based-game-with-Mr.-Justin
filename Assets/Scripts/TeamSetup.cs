using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic;
using System.Linq; // B)

public class TeamSetup : MonoBehaviour
{
    public List<CharAttr> allies;
    public List<float> allyhealths; // Percentage
    // Start is called before the first frame update
    void Awake()
    {
        allyhealths = allies.Select(a => 1f).ToList();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
