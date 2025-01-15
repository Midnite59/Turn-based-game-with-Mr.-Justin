using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic;
using UnityEditor;

[CreateAssetMenu(menuName = "Battle Logic/Character")]
public class CharAttr : ScriptableObject
{
    public string charname;
    public CharStats stats;
    public List<CharSkill> skills;
    public CharSkill fallback = null;
    public Stance characterArt;
    public BattleActor prefab;


    public CharSkill GetBasic() 
    {
        try { return skills[0]; } catch (ArgumentOutOfRangeException) { return fallback; }
    }
    public CharSkill GetSkill1()
    {
        try { return skills[1]; } catch (ArgumentOutOfRangeException) { return fallback; }
    }
    public CharSkill GetSkill2()
    {
        try { return skills[2]; } catch (ArgumentOutOfRangeException) { return fallback; }
    }
}
