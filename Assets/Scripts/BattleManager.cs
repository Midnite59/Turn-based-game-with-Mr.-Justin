using BattleLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager batman;
    public BattleUIController bui;
    public BattleTeam allyTeam;
    public BattleTeam enemyTeam;
    // Start is called before the first frame update
    void Awake()
    {
        if (batman == null)
        {
            batman = this;
        }
        else
        { 
            Destroy(gameObject); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(List<CharAttr> allies, List<CharAttr> enemies) 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        int id = 1;
        for (int i = 0; i < allies.Count; i++)
        {
            allyTeam.batactors[i].id = id++;
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            enemyTeam.batactors[i].id = id++;
        }

        foreach (BattleActor actor in allyTeam.batactors) 
        {
            if (actor.id == 0)
            {
                actor.gameObject.SetActive(false);
            }
        }
        foreach (BattleActor actor in enemyTeam.batactors)
        {
            if (actor.id == 0)
            {
                actor.gameObject.SetActive(false);
            }
        }
    }

}
