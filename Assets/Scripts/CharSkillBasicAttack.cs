using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Mine
using BattleLogic;
using System.Linq;

[CreateAssetMenu(menuName = "Battle Logic/Skill/Basic Attack")]
public class CharSkillEnemyAttack : CharSkill
{
    // Damage
    public float dmg = 15;
    public int hitamount;
    public List<AnimHit> GetHits(GameState gsIN, int user, List<int> targets)
    {
        List<AnimHit> hits = new List<AnimHit>();
        for (int i = 0; i < hitamount; i++) 
        {
            List<AnimHurt> hurts = new List<AnimHurt>();
            foreach (var target in targets) 
            {
                hurts.Add(new AnimHurt(target, (int)(dmg/hitamount)));
            }
            hits.Add(new AnimHit(hurts));
        }
        Debug.Log("Triggered: GetHits()");
        return hits;
    }
    public override GameState Execute(GameState state, Actor user, List<Actor> targets, out BattleFlags flags)
    {
        flags = BattleFlags.None;
        Debug.Log(user.name + " (" + user.id + ") attacked " + string.Join(", ", targets.Select(a => a.name + " (" + a.id + ")").ToList()));
        foreach (var target in targets)
        {
            state = state.WithActor(target.TakeDmg(dmg));
        }
        return state;
    }
    public override Stance GetArt(GameState state, Actor user)
    {
        return user.stance;
    }
    public override OutputEvent GetEvent(GameState gsOUT)
    {
        return new AttackAnimationEvent(gsOUT);
    }
    public override void Animate(GameState gsIN, GameState gsOUT, int userid, List<int> targetids)
    {
        BattleManager.batman.QueueEvent(new AttackAnimationEvent(gsOUT), GetHits(gsIN, userid, targetids));
    }

}
