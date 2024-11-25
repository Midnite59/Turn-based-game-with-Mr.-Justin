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
    public int power = 15;
    public int hitamount;
    private void Awake()
    {
        targetType = TargetType.SingleEnemy;
    }
    public List<AnimHit> GetHits(GameState gsIN, int user, List<int> targets)
    {
        GameState gs = gsIN.Copy();
        List<int> damages = new List<int>();
        foreach (var target in targets) 
        {
            int dmg = Helper.CalcDmg(gsIN.GetActor(target), power, gsIN.GetActor(user), ref gs);
            damages.Add(dmg);
        }
        List<AnimHit> hits = new List<AnimHit>();
        for (int i = 0; i < hitamount; i++) 
        {
            List<AnimHurt> hurts = new List<AnimHurt>();
            for (int j = 0; j < targets.Count(); j++)  
            {
                hurts.Add(new AnimHurt(targets[j], (int)(damages[j]/hitamount)));
            }
            hits.Add(new AnimHit(hurts));
        }
        Debug.Log("Triggered: GetHits()");
        return hits;
    }
    public override GameState Execute(GameState state, Actor user, List<Actor> targets, out BattleFlags flags)
    {
        GameState gs = state.Copy();
        flags = BattleFlags.None;
        Debug.Log(user.name + " (" + user.id + ") attacked " + string.Join(", ", targets.Select(a => a.name + " (" + a.id + ")").ToList()));
        foreach (var target in targets)
        {
            int dmg = Helper.CalcDmg(target, power, user, ref gs);
            state = state.WithActor(target.TakeDmg(dmg));
        }
        return state;
    }
    public override Stance GetArt(GameState state, Actor user)
    {
        return user.stance;
    }
    public override void Animate(GameState gsIN, GameState gsOUT, int userid, List<int> targetids)
    {
        BattleManager.batman.QueueEvent(new AttackAnimationEvent(gsOUT, userid, "BasicAttack"), GetHits(gsIN, userid, targetids));
    }

}
