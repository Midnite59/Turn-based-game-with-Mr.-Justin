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
    public List<int> currentTargets;

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
    public static GameLoop instance;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        currentTargets = new List<int>();
        enemyTurnStart += EnemyTurnVeryBasic;
    }

    /* order 
     1. Attack enemy with certian character who's "specialty stance" determines the stance of the start of the battle
     3. Action is not taken for the cycle
     4. Stance Determines what resource (Stance Points - We can change it later) is regained with skills, 
        and it also increases Efficiency for skills used by specialty characters
     5. Stance points (Title TBD) are the resource that is used to cast skills.


    Stance points are now called "Material ammunition needed for abilities" or M.A.N.A
     
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
            allyActorList.Add(new Actor(allylist[i].charname, allylist[i].stats, id++, allyhealths[i] * allylist[i].stats.Maxhp, allylist[i].characterArt));
        }
        for (int i = 0; i < enemylist.Count(); i++)
        {
            attrList[id] = enemylist[i];
            currentTargets.Add(id); // For Dev Purposes only
            enemyActorList.Add(new Actor(enemylist[i].charname, enemylist[i].stats, id++, enemylist[i].stats.Maxhp, enemylist[i].characterArt));
        }
        //currentTargets.Add(1); pre enemey ai test

        ImmutableList<Actor> allies = ImmutableList.Create<Actor>(allyActorList.ToArray());
        ImmutableList<Actor> enemies = ImmutableList.Create<Actor>(enemyActorList.ToArray());
        gs = new GameState(allies, enemies, null, Stance.Physical, allyStancePoints, enemyStancePoints);
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
        if (skill)
        {
            gs = skill.Execute(gs, currentactor, targets);
        } else
        {
            Debug.LogWarning(gs.currentActor.name + " used splash. (This is a fallback. Are u sure this was intentional?)");
        }
        currentTurn++;
        StartTurn();
    }
    

    public void EnemyTurnVeryBasic()
    {
        TakeTurn(currentAttr.GetBasic(), gs.allies.Select(a => a.id).ToList());
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    /* */
}

