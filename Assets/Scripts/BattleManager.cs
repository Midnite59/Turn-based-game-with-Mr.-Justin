using BattleLogic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager batman;
    public BattleUIController bui;
    public BattleTeam allyTeam;
    public BattleTeam enemyTeam;
    private Coroutine currentEvent;
    public GameState gs;
    private bool eventRunning;
    public List<AnimHit> hitqueue;
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

    public BattleActor GetBattleActor(int id)
    {

        BattleActor batactor = allyTeam.batactors.FirstOrDefault(a => a.id == id);
        if (batactor == null)
        {
            batactor = enemyTeam.batactors.FirstOrDefault(a => a.id == id);
        }
        if (batactor == null) 
        {
            throw new System.NullReferenceException(string.Format("There is no battle actor with id {0} :/", id));
        }
        return batactor;
    }
    IEnumerator RunEvent(OutputEvent oEvent)
    {
        currentEvent = StartCoroutine(oEvent.Execute(gs));
        eventRunning = true;
        yield return currentEvent;
        gs = oEvent.gsOUT;
        eventRunning = false;
        yield break;
    }
    public void HitAnimation() 
    {
        // Triggers the hits in the queue
    }
}
