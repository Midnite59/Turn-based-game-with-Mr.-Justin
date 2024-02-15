using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic;

//[CreateAssetMenu(menuName = "Battle Logic/Skill")]
public class CharSkill : ScriptableObject
{
    //public Stance reqStance;
    public Stance prefStance;
    public TargetType targetType;

    public virtual bool IsRestricted(GameState state, Actor user)
    {
        return false;
    }
    public virtual bool HasEnoughResources(GameState state, Actor user)
    {
        return true;
    }
    public virtual bool HasValidTargets(GameState state, Actor user)
    {
        return true;
    }
    public bool IsUsable(GameState state, Actor user)
    {
        return !IsRestricted(state, user) && HasEnoughResources(state, user) && HasValidTargets(state, user);
    }
    public virtual GameState Execute(GameState state, Actor user, List<Actor> targets)
    {
        Debug.Log("You did not set the override, dummy!");
        throw new NotImplementedException();
    }
}
