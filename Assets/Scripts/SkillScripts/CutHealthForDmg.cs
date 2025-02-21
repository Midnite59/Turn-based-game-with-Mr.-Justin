using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Mine
using BattleLogic;
using System.Linq;

[CreateAssetMenu(menuName = "Battle Logic/Skill/Sacrifice Attack")]
public class CutHealthForDmg : CharSkill
{
    // Damage
    public int power = 30;
    public float sacrifice = 10;
    public override GameState Execute(GameState state, Actor user, List<Actor> targets, out BattleFlags flags)
    {
        flags = BattleFlags.None;
        Debug.Log(user.name + " (" + user.id + ") attacked " + string.Join(", ", targets.Select(a => a.name + " (" + a.id + ")").ToList()));

        state = state.WithActor(user.TakeDmg(Mathf.Min(sacrifice, user.hp - 1), flags));

        foreach (var target in targets)
        {
            state = state.WithActor(target.TakeDmg(Helper.CalcDmg(target, power, user, ref state, out flags, art != Stance.None ? art : null), flags));
        }
        return state;
    }
}