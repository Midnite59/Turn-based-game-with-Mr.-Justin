using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Mine
using BattleLogic;
using System.Linq;

[CreateAssetMenu(menuName = "Battle Logic/Skill/Basic Attack")]
public class CharSkillAttack : CharSkill
{
    // Damage
    public int power = 15;
    [Header("Adds up to one!!")]
    public List<float> hitSplit;
    public int hitamount { get { return hitSplit.Count(); } }
    public List<AnimHit> GetHits(GameState gsIN, int user, List<int> targets)
    {
        List<int> damages = new List<int>();
        foreach (var target in targets) 
        {
            int dmg = Helper.CalcDmg(gsIN.GetActor(target), power, gsIN.GetActor(user), ref gsIN, out _, art != Stance.None ? art : null);
            damages.Add(dmg);
        }
        List<AnimHit> hits = new List<AnimHit>();
        for (int i = 0; i < hitamount; i++) 
        {
            List<AnimHurt> hurts = new List<AnimHurt>();
            for (int j = 0; j < targets.Count(); j++)  
            {
                hurts.Add(new AnimHurt(targets[j], (int)(damages[j] * hitSplit[i]), gsIN.GetActor(user).stance));
            }
            hits.Add(new AnimHit(hurts));
        }
        //Debug.Log("Triggered: GetHits()");
        return hits;
    }
    public override GameState Execute(GameState state, Actor user, List<Actor> targets, out BattleFlags flags)
    {
        state = base.Execute(state, user, targets, out flags);
        //Debug.Log(user.name + " (" + user.id + ") attacked " + string.Join(", ", targets.Select(a => a.name + " (" + a.id + ")").ToList()));
        foreach (var target in targets)
        {
            int dmg = Helper.CalcDmg(target, power, user, ref state, out flags, art != Stance.None ? art:null);
            state = state.WithActor(target.TakeDmg(dmg, flags));
        }
        return state;
    }
    public override Stance GetArt(GameState state, Actor user)
    {
        return user.stance;
    }
    public override void Animate(GameState gsIN, GameState gsOUT, int userid, List<int> targetids)
    {
        AttackAnimationEvent oevent = new AttackAnimationEvent(gsOUT, userid, skilltype);
        if (targetType == TargetType.SingleEnemy)
        {
            if (gsIN.allies.Any(a => { return a.id == userid; }))
            {
                BattleManager.batman.QueueEvent(new CameraEvent(gsIN, () => { BattleManager.batman.batcam.Focus1v1(BattleManager.batman.GetBattleActor(userid), BattleManager.batman.GetBattleActor(targetids[0])); }));
            }
            else
            {
                BattleManager.batman.QueueEvent(new CameraEvent(gsIN, () => { BattleManager.batman.batcam.Focus1v1(BattleManager.batman.GetBattleActor(targetids[0]), BattleManager.batman.GetBattleActor(userid)); }));
            }
            
        }
        BattleManager.batman.QueueEvent(oevent, GetHits(gsIN, userid, targetids));
    }

}
