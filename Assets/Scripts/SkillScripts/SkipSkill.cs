using UnityEngine;
using BattleLogic;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Battle Logic/SkipSkill/Default")]
public class SkipSkill : CharSkill
{
    public override GameState Execute(GameState state, Actor user, List<Actor> targets, out BattleFlags flags)
    {
        flags = BattleFlags.CharSkipped;
        return state;
    }
}
