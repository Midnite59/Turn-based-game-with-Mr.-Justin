using BattleLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle Logic/Buff/Basic")]
public class BuffEffect : ScriptableObject, IBuffEffect
{
    [SerializeField]
    int attackoffset = 0;
    [SerializeField]
    int defenseoffset = 0;
    [SerializeField]
    int speedoffset = 0;
    public virtual int GetAttack(GameState gs, int targetID)
    {
        return attackoffset;
    }

    public virtual int GetDefense(GameState gs, int targetID)
    {
        return defenseoffset;
    }

    public virtual int GetSpeed(GameState gs, int targetID)
    {
        return speedoffset;
    }
}
