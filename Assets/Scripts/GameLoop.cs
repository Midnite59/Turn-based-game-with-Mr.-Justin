using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic;
using System.Linq;
using UnityEditor.PackageManager.UI;
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
    public event Action battleOverStart = () => { };
    //public event Action battleOverEnd = () => { };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /* order 
     1. Attack enemy with certian character who's "specialty stance" determines the stance of the start of the battle
     3. Action is taken for the cycle
     4. Stance Determines what resource (Stance Points - We can change it later) is regained with skills, 
        and it also increases Efficiency for skills used by specialty characters
     5. Stance points (Title TBD) are the resource that is used to cast skills.
     
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
            case State.AllyTurn: allyTurnStart.Invoke(); break;
            case State.EndOfRound: endOfRoundStart.Invoke(); RoundEnd(); break;
            case State.BattleOver: battleOverStart.Invoke(); break;
            case State.Interruptions: interuptionsStart.Invoke(); break;
            case State.Cutscene: cutsceneStart.Invoke(); break;
        }
        return true;
    }

    public void CreateBattle(IEnumerable<Actor> allylist, IEnumerable<Actor> enemylist)
    {
        var allyStancePoints = ImmutableDictionary.Create<Stance, float>();
        var enemyStancePoints = ImmutableDictionary.Create<Stance, float>();
        ImmutableList<Actor> allies = ImmutableList.Create<Actor>(allylist.ToArray());
        ImmutableList<Actor> enemies = ImmutableList.Create<Actor>(enemylist.ToArray());
        gs = new GameState(allies, enemies, null, Stance.Range, allyStancePoints, enemyStancePoints);
        StartBattle();
    }

    public void StartBattle()
    {
        startBattleStart.Invoke();
        SetTurnOrder();
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
        int ActorID = turnOrder[currentTurn];
        gs = gs.SetCurrentActor(ActorID);
        if (gs.allies.Any(a => a.id == ActorID))
        {
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
        var spdIDs = actors.Select(actor => new {id = actor.id, spd = actor.stats.spd});
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
        var targetsIE = targetIDs.Select(tid => gs.actors.First(a => a.id == tid));
        var targets = targetsIE.ToList();
        gs = skill.Execute(gs, currentactor, targets);
        currentTurn++;
        StartTurn();
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
    /* */
}

