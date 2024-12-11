using BattleLogic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle Logic/Skill/Heal Actor")]
public class HealActor: CharSkill
{
    /*
        
    */
    public int minhealamount;
    public float healpower;
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
        //BattleManager.batman.QueueEvent(new AttackAnimationEvent(gsOUT, userid, "BasicAttack"), GetHits(gsIN, userid, targetids));
    }
}
