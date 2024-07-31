using BattleLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputEvent
{
    public GameState gsOUT;
    public virtual IEnumerator Execute(GameState gsIN)
    {
        throw new NotImplementedException("No override!");
    }
}

public class AnimationEvent : OutputEvent
{
    public override IEnumerator Execute(GameState gsIN) 
    {
        // Do animation stuff
        yield break;
    }
}

public class UIEvent : OutputEvent 
{
    public override IEnumerator Execute(GameState gsIN)
    {
        // Do UI stuff
        yield break;
    }
}

public class AttackAnimationEvent : AnimationEvent
{

}

public class AnimHurt 
{
    int targetid;
    int dmg;
}

public class AnimHit
{
    List<AnimHurt> animHurts;
}