using BattleLogic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle Logic/Skill/Buff Actor")]
public class BuffActor : CharSkill
{
    /*
        
    */
    public BuffEffect effect;
    public int duration;
    public override GameState Execute(GameState state, Actor user, List<Actor> targets, out BattleFlags flags)
    {
        flags = BattleFlags.None;
        foreach (var target in targets)
        {
            state = state.WithActor(target.WithBuff(new Buff(duration, effect, user.id)));
        }
        return state;
    }
    public override Stance GetArt(GameState state, Actor user)
    {
        return user.stance;
    }
    public override void Animate(GameState gsIN, GameState gsOUT, int userid, List<int> targetids)
    {
        BattleManager.batman.QueueEvent(new NonAttackAnimationEvent(gsOUT, userid, skilltype));
    }
}
