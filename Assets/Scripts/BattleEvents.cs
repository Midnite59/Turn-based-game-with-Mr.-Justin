using BattleLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputEvent
{
    public OutputEvent(GameState gsOUT) 
    {
        this.gsOUT = gsOUT;
    }

    public GameState gsOUT;
    public virtual IEnumerator Execute(GameState gsIN)
    {
        Debug.Log("No override, making the character look silly by doing nothing");
        yield break;
    }
}

public class AnimationEvent : OutputEvent
{
    public AnimationEvent(GameState gsOUT, int userid, string triggername) : base(gsOUT) 
    {
        this.userid = userid;
        this.triggername = triggername;
    }
    int userid;
    string triggername;
    public override IEnumerator Execute(GameState gsIN) 
    {
        BattleActor batactor = BattleManager.batman.GetBattleActor(userid);
        batactor.animator.SetTrigger(triggername);
        //Debug.Log("Execute() Triggered");
        yield return new WaitForSeconds(3);
        yield break;
    }
}

public class UIEvent : OutputEvent 
{
    public UIEvent(GameState gsOUT) : base(gsOUT) { }
    public override IEnumerator Execute(GameState gsIN)
    {
        // Do UI stuff
        yield return null;
        yield break;
    }
}

public class BuffUIEvent : UIEvent
{
    public BuffUIEvent(GameState gsOUT) : base(gsOUT) { }
}

public class AttackAnimationEvent : AnimationEvent
{
    public AttackAnimationEvent(GameState gsOUT, int userid, string triggername) : base(gsOUT, userid, triggername) { }
}

public class NonAttackAnimationEvent : AnimationEvent
{
    public NonAttackAnimationEvent(GameState gsOUT, int userid, string triggername) : base(gsOUT, userid, triggername) { }
}

public class AnimHurt 
{
    public AnimHurt(int targetid, int dmg)
    {
        this.targetid = targetid;
        this.dmg = dmg;
    }
    public int targetid;
    public int dmg;
}

public class AnimHit
{
    public AnimHit(List<AnimHurt> animHurts)
    {
        this.animHurts = animHurts;
    }
    public List<AnimHurt> animHurts;
}