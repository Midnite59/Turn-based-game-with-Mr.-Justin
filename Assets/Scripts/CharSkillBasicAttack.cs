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
    public override GameState Execute(GameState state, Actor user, List<Actor> targets)
    {
        Debug.Log(user.name + " (" + user.id + ") attacked " + string.Join(", ", targets.Select(a => a.name + " (" + a.id + ")").ToList()));
        foreach (var target in targets)
        {
            state = state.WithActor(target.TakeDmg(dmg));
        }
        return state;
    }
}
