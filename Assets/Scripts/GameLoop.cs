using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic;
using System.Linq;
using System.Collections.Immutable;

public class GameLoop : MonoBehaviour
{

    public enum State 
    {
        StartBattle, AllyTurn, EnemyTurn, EndOfRound, Interruptions, Cutscene, BattleOver
    }
    public State currentState = State.StartBattle;
    //public List<int> idList; //ids
    //public List<int> speedList; //stats.speed
    public int currentTurn; //index
    public List<int> turnOrder; //ids
    public GameState gs;
    public Dictionary<int, CharAttr> attrList;
    public CharAttr currentAttr { get { return attrList[turnOrder[currentTurn]]; } }

    public event Action startBattleStart = () => { };
    public event Action startBattleEnd = () => { };
    public event Action allyTurnStart = () => { };
    public event Action allyTurnEnd = () => { };
    public event Action enemyTurnStart = () => { };
    public event Action enemyTurnEnd = () => { };
    public event Action endOfRoundStart = () => { };
    public event Action endOfRoundEnd = () => { };
    public event Action interuptionsStart = () => { };
    public event Action interuptionsEnd = () => { };
    public event Action cutsceneStart = () => { };
    public event Action cutsceneEnd = () => { };
    public event Action battleOverWin = () => { };

    public System.Random random = new System.Random();

    private List<BattleEvent> battleEvents = new List<BattleEvent>();

    public bool battleResult;

    public event Action battleOverLose = () => { };
    public static GameLoop instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        enemyTurnStart += EnemyTurnVeryBasic;
        //Debug.Log(instance);
    }

    public void EventStack(BattleEvent bEvent)
    {
        battleEvents = battleEvents.Prepend(bEvent).ToList();
    }
    public void EventQueue(BattleEvent bEvent)
    {
        battleEvents = battleEvents.Append(bEvent).ToList();
    }

    /* 
     Todo:
        Stats and make them work
        Interuptions
     
    */
    bool TransitionValid(State newState)
    {
        switch (currentState)
        {
            case State.StartBattle: return newState == State.AllyTurn || newState == State.EnemyTurn || newState == State.Cutscene || newState == State.Interruptions;
            case State.EnemyTurn: return newState == State.EnemyTurn || newState == State.AllyTurn || newState == State.BattleOver || newState == State.EndOfRound || newState == State.Interruptions || newState == State.Cutscene;
            case State.AllyTurn: return newState == State.EnemyTurn || newState == State.AllyTurn || newState == State.BattleOver || newState == State.EndOfRound || newState == State.Interruptions || newState == State.Cutscene;
            case State.EndOfRound: return newState == State.EnemyTurn || newState == State.AllyTurn || newState == State.BattleOver || newState == State.Cutscene;
            case State.BattleOver: return false;
            case State.Interruptions: return newState == State.EnemyTurn || newState == State.AllyTurn || newState == State.BattleOver || newState == State.EndOfRound || newState == State.Interruptions || newState == State.Cutscene;
            case State.Cutscene: return newState == State.EnemyTurn || newState == State.AllyTurn || newState == State.BattleOver || newState == State.EndOfRound || newState == State.Interruptions;
            default : throw new NotImplementedException();
        }
    }

    bool TransitionState(State newstate)
    {
        if (!TransitionValid(newstate)) return false;
        switch (currentState) 
        {
            case State.StartBattle: startBattleEnd.Invoke(); break;
            case State.EnemyTurn: enemyTurnEnd.Invoke(); break;
            case State.AllyTurn: allyTurnEnd.Invoke(); break;
            case State.EndOfRound: endOfRoundEnd.Invoke(); break;
            //case State.BattleOver: battleOverEnd.Invoke(); break;
            case State.Interruptions: interuptionsEnd.Invoke(); break;
            case State.Cutscene: cutsceneEnd.Invoke(); break;
        }
        currentState = newstate;
        switch (currentState)
        {
            //case State.StartBattle: startBattleStart.Invoke(); break;
            case State.EnemyTurn: enemyTurnStart.Invoke(); break;
            case State.AllyTurn: allyTurnStart.Invoke(); Debug.Log("invoked"); break;
            case State.EndOfRound: endOfRoundStart.Invoke(); RoundEnd(); break;
            case State.BattleOver: if (battleResult) battleOverWin.Invoke(); else battleOverLose.Invoke(); break;
            case State.Interruptions: interuptionsStart.Invoke(); break;
            case State.Cutscene: cutsceneStart.Invoke(); break;
        }
        return true;
    }

    public void CreateBattle(List<CharAttr> allylist, List<CharAttr> enemylist, List<float> allyhealths)
    {
        attrList = new Dictionary<int, CharAttr>();
        var allyStancePoints = ImmutableDictionary.Create<Stance, float>();
        var enemyStancePoints = ImmutableDictionary.Create<Stance, float>();
        int id = 1;
        List<Actor> allyActorList = new List<Actor>();
        List<Actor> enemyActorList = new List<Actor>();

        for (int i = 0; i < allylist.Count(); i++) 
        {
            attrList[id] = allylist[i];
            allyActorList.Add(new Actor(allylist[i].charname, allylist[i].stats, id++, allyhealths[i] * allylist[i].stats.Maxhp, allylist[i].characterArt, new ActorStatus()));
        }
        for (int i = 0; i < enemylist.Count(); i++)
        {
            attrList[id] = enemylist[i]; 
            enemyActorList.Add(new Actor(enemylist[i].charname, enemylist[i].stats, id++, enemylist[i].stats.Maxhp, enemylist[i].characterArt, new ActorStatus()));
        }
       

        ImmutableList<Actor> allies = ImmutableList.Create<Actor>(allyActorList.ToArray());
        ImmutableList<Actor> enemies = ImmutableList.Create<Actor>(enemyActorList.ToArray());
        gs = new GameState(allies, enemies, null, Stance.Physical, allyStancePoints, enemyStancePoints);
        BattleManager.batman.Setup(allylist, enemylist);
        StartBattle();
    }

    public void StartBattle()
    {
        startBattleStart.Invoke(); //DISTURBING THE PEACE!!!
        SetTurnOrder();
        BattleManager.batman.StartBatman(gs);
        StartTurn();
    }

    public void RoundEnd() 
    {
        Debug.Log("Round Over!");
        SetTurnOrder();
        currentTurn = 0;
        StartTurn();
    }

    public void StartTurn()
    {
        if (currentTurn >= turnOrder.Count)
        {
            TransitionState(State.EndOfRound);
            return;
        }
            while (gs.GetActor(turnOrder[currentTurn]).status.dead)
            {
                currentTurn++;
                if (currentTurn >= turnOrder.Count)
                {
                    TransitionState(State.EndOfRound);
                    return;
                }
            }


        int ActorID = turnOrder[currentTurn];
        gs = gs.SetCurrentActor(ActorID);
        gs = gs.WithActor(gs.currentActor.WithStatus(gs.currentActor.status.Recover()));
        if (gs.allies.Any(a => a.id == ActorID))
        {
            //Debug.Log("oops");
            TransitionState(State.AllyTurn);
            return;
        } else if (gs.enemies.Any(a => a.id == ActorID)) 
        {
            
            TransitionState(State.EnemyTurn);
            return;
        } else 
        { 
            throw new ArgumentOutOfRangeException("No ID matched any list :c"); // No ID matched any list :c
        }
    }

    public void SetTurnOrder()
    {
        turnOrder.Clear();
        var actors = gs.actors;
        var spdIDs = actors.Select(actor => new {id = actor.id, spd = actor.stats.spd * Helper.StageToMulti(actor.Mspd)});
        while (spdIDs.Any(a => a.spd > 0)) 
        {
            var fastest = spdIDs.First(a => a.spd == spdIDs.Max(a => a.spd));
            turnOrder.Add(fastest.id);
            fastest = new { id = fastest.id, spd = fastest.spd - 100 };
            spdIDs = spdIDs.Select(a => a.id == fastest.id ? fastest: a);
        } 
        
    }
    public void TakeTurn(CharSkill skill, List<int> targetIDs)
    {
        int ActorID = turnOrder[currentTurn];
        var currentactor = gs.currentActor;
        Debug.Log(targetIDs.Count + " targets");
        Debug.Log(String.Join(", ", targetIDs));
        var targetsIE = targetIDs.Select(tid => gs.actors.First(a => a.id == tid));
        var targets = targetsIE.ToList();
        if (skill)
        {
            BattleFlags flags = BattleFlags.None;
            GameState animgs = gs;
            gs = skill.Execute(gs, currentactor, targets, out flags);
            skill.Animate(animgs, gs, currentactor.id, targetIDs);
            if ((flags & BattleFlags.CharDowned) == BattleFlags.CharDowned)
            {
                //Debug.Log(String.Join(", " ,targets.Select(a => a.name)) + " was downed :O");
                gs = gs.WithStance(skill.art);
                
            }
        } else
        {
            Debug.LogWarning(gs.currentActor.name + " used splash. (This is a fallback. Are u sure this was intentional?)");
        }
        //post action event
        bool? dead = Check4Dead();
        if (dead.HasValue)
        {
            battleResult = dead.Value;
            TransitionState(State.BattleOver);
        }
        else
        {
            currentTurn++;
            StartTurn();
        }
    }


    void ProcessEvents()
    {
        while (battleEvents.Count > 0)
        {
            BattleEvent bevent = battleEvents[0];
            if (bevent == null)
            {
                Debug.LogError("You queued a null event. How did you even do that?");
                throw new NullReferenceException("You queued a null event. How did you even do that?");
            }
            switch(bevent.type) 
            {
                case BattleEvent.Type.Down: //down
                    break;
                case BattleEvent.Type.Dead: //dead
                    break;
            }
        }
    }


    public void EnemyTurnVeryBasic()
    {
        List<int> allyids = gs.allies.Select(a => a.id).ToList();
        List<int> targetids = new List<int>();
        targetids.Add(allyids[random.Next(gs.allies.Count)]);
        TakeTurn(currentAttr.GetBasic(), targetids);
    }
    
    // Update is called once per frame
    void Update()
    {
        //instance = this;
    }
    /* */


    bool? Check4Dead()
    {
        if (gs.allies.All(a => a.status.dead))
        {
            Debug.Log("You died you loser");
            return false;
        }
        else if (gs.enemies.All(a => a.status.dead))
        {
            return true;
        }
        else
        {
            return null;
        }
    }
}

