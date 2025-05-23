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
    public AnimationEvent(GameState gsOUT, int userid, string paramname, float delay /*, trigger? triggerValue = null (wait a second) */) : base(gsOUT) 
    {
        this.userid = userid;
        this.delay = delay;
        this.paramname = paramname;
      //this.triggerValue = triggerValue
    }
    public AnimationEvent(GameState gsOUT, int userid, string paramname, float delay, bool? boolValue = null) : base(gsOUT) 
    {
        this.userid = userid;
        this.delay = delay;
        this.paramname = paramname;
        this.boolValue = boolValue;
    }
    public AnimationEvent(GameState gsOUT, int userid, string paramname, float delay, float? floatValue = null) : base(gsOUT)
    {
        this.userid = userid;
        this.delay = delay;
        this.paramname = paramname;
        this.floatValue = floatValue;
    }
    public AnimationEvent(GameState gsOUT, int userid, string paramname, float delay, int? intValue = null) : base(gsOUT)
    {
        this.userid = userid;
        this.delay = delay;
        this.paramname = paramname;
        this.intValue = intValue;
    }
    int userid;
    float delay;
    string paramname;
    bool? boolValue;
    float? floatValue;
    int? intValue;
    //trigger? triggerValue;
    public override IEnumerator Execute(GameState gsIN) 
    {
        BattleActor batactor = BattleManager.batman.GetBattleActor(userid);
        if (boolValue.HasValue) 
        {
            batactor.animator.SetBool(paramname, boolValue.Value);
        }
        else if (floatValue.HasValue)
        {
            batactor.animator.SetFloat(paramname, floatValue.Value);
        }
        else if (intValue.HasValue)
        {
            batactor.animator.SetInteger(paramname, intValue.Value);
        }
        else
        {
            batactor.animator.SetTrigger(paramname);
        }
            //Debug.Log("Execute() Triggered");
            yield return new WaitForSeconds(delay);
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
    public AttackAnimationEvent(GameState gsOUT, int userid, string triggername, float delay = 3) : base(gsOUT, userid, triggername, delay) { }
}

public class NonAttackAnimationEvent : AnimationEvent
{
    public NonAttackAnimationEvent(GameState gsOUT, int userid, string triggername, float delay = 3) : base(gsOUT, userid, triggername, delay) { }
}

public class AnimHurt 
{
    public AnimHurt(int targetid, int dmg, Stance stance)
    {
        this.targetid = targetid;
        this.dmg = dmg;
        this.stance = stance;
    }
    public int targetid;
    public int dmg;
    public Stance stance;
}

public class AnimHit
{
    public AnimHit(List<AnimHurt> animHurts)
    {
        this.animHurts = animHurts;
    }
    public List<AnimHurt> animHurts;
}