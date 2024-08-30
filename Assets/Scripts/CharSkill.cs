using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic;

//[CreateAssetMenu(menuName = "Battle Logic/Skill")]
public class CharSkill : ScriptableObject
{
    public TargetType targetType;
    public Sprite icon;

    public Stance art;
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
    public virtual Sprite GetIcon(GameState state, Actor user)
    {
        return null;
    }
    public virtual Stance GetArt(GameState state, Actor user) 
    {
        return art;
    }
    public virtual void Animate(GameState gsIN, GameState gsOUT, int userid, List<int> targetids)
    {
        Debug.LogError("You did not set the override, dummy!");
        BattleManager.batman.QueueEvent(GetEvent(gsOUT));
        // I dont have to return anything! >:)
    }
    public virtual GameState Execute(GameState state, Actor user, List<Actor> targets, out BattleFlags flags)
    {
        Debug.LogError("You did not set the override, dummy!");
        throw new NotImplementedException("Make sure you fill in all of the parameters.");
    }
    public virtual OutputEvent GetEvent(GameState gsOUT) 
    {
        return new OutputEvent(gsOUT);
    }
}
