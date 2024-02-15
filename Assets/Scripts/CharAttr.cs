using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic;

[CreateAssetMenu(menuName = "Battle Logic/Character")]
public class CharAttr : ScriptableObject
{
    public string charname;
    public CharStats stats;
    public List<CharSkill> skills;
}
