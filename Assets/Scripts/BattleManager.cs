using BattleLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager batman;
    public BattleUIController bui;
    public BattleTeam allyTeam;
    public BattleTeam enemyTeam;
    public List<BattleActor> allactors { get { return allyTeam.batactors.Concat(enemyTeam.batactors).ToList(); } }
    private Coroutine currentEvent;
    public GameState gs;
    private bool eventRunning;
    public List<AnimHit> hitqueue;
    public List<OutputEvent> outputevents;
    public int selectTarget;
    public int lastSelectedEnemy;
    public CharSkill selectedSkill;
    public List<int> realTargets;
    public bool allyTurnStart;
    private bool batmanInit;
    private bool batmanSetup;

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
        outputevents = new List<OutputEvent>();
        hitqueue = new List<AnimHit>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        bui.enabled = batmanInit && batmanSetup;
        if (batmanInit && batmanSetup)
        {
            ProcessEvents();
        }
        //Debug.Log(gs.currentActor.id);
    }
    public void StartBatman(GameState startGS)
    {
        gs = startGS;
        batmanInit = true;
    }

    public void Setup(List<CharAttr> allies, List<CharAttr> enemies)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        int id = 1;
        for (int i = 0; i < allies.Count; i++)
        {
            allyTeam.batactors[i].id = id++;
            allyTeam.batactors[i].hp = allies[i].stats.Maxhp;
            allyTeam.batactors[i].charname = allies[i].charname;
            allyTeam.batactors[i].name = allies[i].charname;
            allyTeam.batactors[i].basic = allies[i].GetBasic();
            allyTeam.batactors[i].skill1 = allies[i].GetSkill1();
            allyTeam.batactors[i].skill2 = allies[i].GetSkill2();
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            enemyTeam.batactors[i].id = id++;
            enemyTeam.batactors[i].hp = enemies[i].stats.Maxhp;
            enemyTeam.batactors[i].charname = enemies[i].charname;
            enemyTeam.batactors[i].name = enemies[i].charname;
            enemyTeam.batactors[i].basic = enemies[i].GetBasic();
            enemyTeam.batactors[i].skill1 = enemies[i].GetSkill1();
            enemyTeam.batactors[i].skill2 = enemies[i].GetSkill2();
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
        batmanSetup = true;
        selectTarget = lastSelectedEnemy = enemyTeam.batactors[0].id;
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
        UntargetAll();
        Debug.Log(currentEvent);
        yield return currentEvent;
        Debug.Log(currentEvent);
        eventRunning = false;
        yield break;
    }

    public List<BattleActor> GetBattleActors(List<int> ids) 
    {
       return allactors.Where(a => ids.Contains(a.id)).ToList();
    }
    public void ShowTargets(List<int> ids) 
    {
        allactors.ForEach(a => a.UnTarget());
        var targets = GetBattleActors(ids);
        targets.ForEach(a => a.Target());
    }
    public void HitAnimation() 
    {
        // Triggers the hits in the queue
        if (hitqueue.Count == 0)
        { 
            Debug.LogWarning("Not enough hits in hitqueue :O"); 
        }
        hitqueue[0].animHurts.ForEach(a => { GetBattleActor(a.targetid).HurtAnimation(a.dmg); Debug.Log("ouch! " + a.dmg + " damage!! - " + a.targetid); });
        
        hitqueue.RemoveAt(0);
    }
    public void QueueEvent(OutputEvent oEvent, List<AnimHit> hits = null)
    {
        outputevents.Add(oEvent);
        if (hits != null)
        {
            hitqueue = hitqueue.Concat(hits).ToList();
            Debug.LogWarning(hits.Count + " Hits (supposedly) added");
        } else
        {
            Debug.LogWarning("No hits added");
        }
    }
    public void ProcessEvents()
    {
        if (!eventRunning)
        {
            if (currentEvent != null) 
            {
                UpdateGs(outputevents[0].gsOUT);
                outputevents.RemoveAt(0);
                currentEvent = null;
                Debug.Log("No event running :D. Finally I can take a break");
            }
            if (outputevents.Count > 0) 
            {
                StartCoroutine(RunEvent(outputevents[0]));
            }
            else
            {
                if (GameLoop.instance.currentState == GameLoop.State.AllyTurn)
                {
                    if (!allyTurnStart)
                    {
                        gs = GameLoop.instance.gs;
                        selectedSkill = GetBattleActor(gs.currentActor.id).basic;
                        Debug.Log(selectedSkill.name);
                        allyTurnStart = true;
                        SelectTargets();
                        ShowTargets(BattleManager.batman.realTargets);
                    }
                }
            }
        } 
        else
        {
            allyTurnStart = false;
        }
    }
    void UpdateGs(GameState gsOut)
    {
        gs = gsOut;
        // UI stuff
    }

    public void TargetChange(bool right, bool ally = false)
    {
        if (selectedSkill.targetType == TargetType.Self) 
        {
            Debug.Log("nice try");
            return;
        }
        BattleTeam team = ally ? allyTeam : enemyTeam;
        int currentindex = team.batactors.IndexOf(GetBattleActor(selectTarget));
        if (currentindex != -1)
        {
            if (right)
            {
                if (currentindex + 1 < team.batactors.Where(a => a.id > 0).Count())
                {
                    currentindex++;
                }
                else
                {
                    Debug.Log("no");
                }
            }
            else
            {
                if (currentindex - 1 > -1)
                {
                    currentindex--;
                }
                else
                {
                    Debug.Log("no");
                }
            }
            selectTarget = team.batactors[currentindex].id;
            UntargetAll();
            SelectTargets();
            ShowTargets(realTargets);
            //Change based on targetype
            if (!ally) { lastSelectedEnemy = selectTarget; }
        }
        else
        {
            throw new NotImplementedException("Tried to select from nonexistant actor :'|");
        }
    }
    void UntargetAll() 
    {
        foreach (BattleActor actor in allyTeam.batactors)
        {
            actor.UnTarget();
        }
        foreach (BattleActor actor in enemyTeam.batactors)
        {
            actor.UnTarget();
        }
    }

    public void SelectTargets()
    {
        realTargets = new List<int>();
        switch (selectedSkill.targetType) 
        {
            case TargetType.Self: 
            case TargetType.SingleAlly:
            case TargetType.BurstAlly:
            case TargetType.AoeAlly:
            case TargetType.AoeAll:
                if (!allyTeam.batactors.Select(a => a.id).Contains(selectTarget)){
                    selectTarget = gs.currentActor.id;
                } break;
            case TargetType.SingleEnemy:
            case TargetType.BurstEnemy:
            case TargetType.AoeEnemy:
                if (!enemyTeam.batactors.Select(a => a.id).Contains(selectTarget))
                {
                    if (lastSelectedEnemy > 0)
                    {
                        selectTarget = lastSelectedEnemy;
                    }
                    else
                    {
                        selectTarget = enemyTeam.batactors[0].id;
                        Debug.Log("im working: " + selectedSkill.targetType);
                    }
                } break;
        }
        switch (selectedSkill.targetType)
        {
            case TargetType.SingleAlly: realTargets.Add(selectTarget); break;
            case TargetType.BurstAlly:
                List<int> allyIDList = allyTeam.batactors.Select(a => a.id).ToList();
                int selectedAllyIndex = allyIDList.FindIndex(a => a == selectTarget);
                realTargets.Add(selectTarget);
                if (selectedAllyIndex + 1 < allyIDList.Count())
                {
                    realTargets.Add(allyIDList[selectedAllyIndex + 1]);
                }
                if (selectedAllyIndex - 1 >= 0)
                {
                    realTargets.Add(allyIDList[selectedAllyIndex - 1]);
                }

                break;
            case TargetType.AoeAlly: realTargets = allyTeam.batactors.Select(a => a.id).ToList(); break;
            case TargetType.AoeAll: realTargets = allactors.Select(a => a.id).ToList(); break;
            case TargetType.SingleEnemy: realTargets.Add(selectTarget); break;
            case TargetType.BurstEnemy:
                List<int> enemyIDList = enemyTeam.batactors.Select(a => a.id).ToList();
                int selectedEnemyIndex = enemyIDList.FindIndex(a => a == selectTarget);
                realTargets.Add(selectTarget);
                if (selectedEnemyIndex + 1 < enemyIDList.Count())
                {
                    realTargets.Add(enemyIDList[selectedEnemyIndex + 1]);
                }
                if (selectedEnemyIndex - 1 >= 0)
                {
                    realTargets.Add(enemyIDList[selectedEnemyIndex - 1]);
                }

                break;
            case TargetType.AoeEnemy: realTargets = enemyTeam.batactors.Select(a => a.id).ToList(); break;
        }
    }
}
