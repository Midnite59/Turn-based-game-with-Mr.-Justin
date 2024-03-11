using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using BattleLogic;
using System.Linq;

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
        Actor sample = new Actor("sample", new CharStats(100, 15, 15, 1, 10), 1, 100, 15, 15, 1, 10);
        Actor sample2 = new Actor("sample2", new CharStats(100, 15, 15, 1, 10), 2, 100, 15, 15, 1, 10);
        ImmutableList<Actor> allies = ImmutableList.Create<Actor>();
        allies = allies.Add(sample);
        ImmutableList<Actor> enemies = ImmutableList.Create<Actor>();
        enemies = enemies.Add(sample2);
        var allyStancePoints = ImmutableDictionary.Create<Stance, float>();
        var enemyStancePoints = ImmutableDictionary.Create<Stance, float>();
        GameState gs = new GameState(allies, enemies, sample, Stance.Physical, allyStancePoints, enemyStancePoints);
        GameState gs2;
        if (testSkill.IsUsable(gs, sample2))
        {
            gs2 = testSkill.Execute(gs, sample2, new List<Actor> { sample });
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
        Actor sample = new Actor(Norman.charname, Norman.stats, 1, 100);
        Actor sample2 = new Actor(NormanButSlow.charname, NormanButSlow.stats, 2, 40);
        Actor sample3 = new Actor(NormanButEvenSlower.charname, NormanButEvenSlower.stats, 3, 50);
        Actor MarlanActor = new Actor(Marlan.charname, Marlan.stats, 4, 130);
        ImmutableList<Actor> allies = ImmutableList.Create<Actor>();
        allies = allies.Add(sample);
        allies = allies.Add(MarlanActor);
        ImmutableList<Actor> enemies = ImmutableList.Create<Actor>();
        enemies = enemies.Add(sample2);
        enemies = enemies.Add(sample3);
        var allyStancePoints = ImmutableDictionary.Create<Stance, float>();
        var enemyStancePoints = ImmutableDictionary.Create<Stance, float>();
        GameState gs = new GameState(allies, enemies, sample, Stance.Physical, allyStancePoints, enemyStancePoints);
        gameloop.gs = gs;
        gameloop.StartBattle();
    }
    private void Start()
    {
        Invoke("Case3", 0.1f);
    }

    private void Case3()
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
