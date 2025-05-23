using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using BattleLogic;
using System.Linq;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

public class IamTest : MonoBehaviour
{
    public CharSkill testSkill;
    public CharSkill MarlanAtk;
    public CharAttr Marlan;
    public CharAttr Norman;
    public CharAttr NormanButSlow;
    public CharAttr NormanButEvenSlower;
    public GameLoop gameloop;
    public EncountersOhNo Encounter;
    public TeamSetup Team;
    // Start is called before the first frame update
    void TestCaseNoGameLoop()
    {
        Actor sample = new Actor("sample", new CharStats(100, 15, 15, 1, 10), 1, 100, Stance.None, new ActorStatus(), ImmutableList<Buff>.Empty);
        Actor sample2 = new Actor("sample2", new CharStats(100, 15, 15, 1, 10), 2, 100, Stance.None, new ActorStatus(), ImmutableList<Buff>.Empty);
        ImmutableList<Actor> allies = ImmutableList.Create<Actor>();
        allies = allies.Add(sample);
        ImmutableList<Actor> enemies = ImmutableList.Create<Actor>();
        enemies = enemies.Add(sample2);
        var allyStancePoints = 5;
        var enemyStancePoints = 5;
        GameState gs = new GameState(allies, enemies, sample, Stance.Physical, allyStancePoints, enemyStancePoints);
        GameState gs2;
        if (testSkill.IsUsable(gs, sample2))
        {
            gs2 = testSkill.Execute(gs, sample2, new List<Actor> { sample }, out _);
        }
        else
        {
            gs2 = gs;
        }
        Debug.Log(gs.currentActor.hp);
        Debug.Log(gs2.currentActor.hp);
    }
    void TestCase2()
    {
        Actor sample = new Actor(Norman.charname, Norman.stats, 1, 100, Stance.None, new ActorStatus(), ImmutableList<Buff>.Empty);
        Actor sample2 = new Actor(NormanButSlow.charname, NormanButSlow.stats, 2, 40, Stance.None, new ActorStatus(), ImmutableList<Buff>.Empty);
        Actor sample3 = new Actor(NormanButEvenSlower.charname, NormanButEvenSlower.stats, 3, 50, Stance.None, new ActorStatus(), ImmutableList<Buff>.Empty);
        Actor MarlanActor = new Actor(Marlan.charname, Marlan.stats, 4, 130, Stance.None, new ActorStatus(), ImmutableList<Buff>.Empty);
        ImmutableList<Actor> allies = ImmutableList.Create<Actor>();
        allies = allies.Add(sample);
        allies = allies.Add(MarlanActor);
        ImmutableList<Actor> enemies = ImmutableList.Create<Actor>();
        enemies = enemies.Add(sample2);
        enemies = enemies.Add(sample3);
        var allyStancePoints = 5;
        var enemyStancePoints = 5;
        GameState gs = new GameState(allies, enemies, sample, Stance.Physical, allyStancePoints, enemyStancePoints);
        gameloop.gs = gs;
        gameloop.StartBattle();
    }
    private void Start()
    {
        Invoke("Case3", 6f);
    }

    public void Case3()
    {
        gameloop.CreateBattle(Team.allies, Encounter.enemies, Team.allyhealths);
    }


    // Update is called once per frame

    public void TakeTurnButtonEdition()
    {
        //gameloop.TakeTurn(testSkill, gameloop.gs.enemies.Select(a => a.id).ToList());
    }



    void Update()
    {
        
    }
}
