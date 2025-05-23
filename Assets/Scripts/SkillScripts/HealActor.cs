using BattleLogic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

[CreateAssetMenu(menuName = "Battle Logic/Skill/Heal Actor")]
public class HealActor: CharSkill
{
    /*
        
    */
    public int minhealamount;
    public int healpower;
    [Header("Adds up to one!!")]
    public List<float> hitSplit;
    public int hitamount { get { return hitSplit.Count(); } }
    public List<AnimHit> GetHits(GameState gsIN, int user, List<int> targets)
    {
        List<int> damages = new List<int>();
        foreach (var target in targets)
        {
            int level = 1;
            int heal = minhealamount + (level * healpower);
            damages.Add(heal);
        }
        List<AnimHit> hits = new List<AnimHit>();
        for (int i = 0; i < hitamount; i++)
        {
            List<AnimHurt> hurts = new List<AnimHurt>();
            for (int j = 0; j < targets.Count(); j++)
            {
                hurts.Add(new AnimHurt(targets[j], (int)(damages[j] * hitSplit[i]), Stance.Super));
            }
            hits.Add(new AnimHit(hurts));
        }
        Debug.Log("Triggered: GetHits()");
        return hits;
    }
    public override GameState Execute(GameState state, Actor user, List<Actor> targets, out BattleFlags flags)
    {
        flags = BattleFlags.None;
        foreach (var target in targets)
        {
            int level = 1;
            state = state.WithActor(target.HealDmg(minhealamount+(level*healpower)));
        }
        return state;
    }
    public override Stance GetArt(GameState state, Actor user)
    {
        return user.stance;
    }
    public override void Animate(GameState gsIN, GameState gsOUT, int userid, List<int> targetids)
    {
        BattleManager.batman.QueueEvent(new NonAttackAnimationEvent(gsOUT, userid, skilltype), GetHits(gsIN, userid, targetids));
    }
}
