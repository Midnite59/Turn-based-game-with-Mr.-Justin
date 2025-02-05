using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthyBar : MonoBehaviour
{
    public BattleActor battleActor;
    public Image greenBar;
    public Image redBar;
    public Image blueBar;
    public float slowBarTime;
    public float fastBarTime;
    public float bActorLastHP;
    public float redBarHP;
    public float greenBarHP;
    public float blueBarHP;
    public float BarDelay;

    // Timers
    float fastBarTimer;
    float slowBarTimer;

    public AnimationCurve fastBarCurve;
    public AnimationCurve slowBarCurve;

    private Coroutine GreenBarRoutine = null;
    private Coroutine RedBarRoutine = null;
    private Coroutine BlueBarRoutine = null;

    private bool slowBarLock;
    public void SetBattleActor(BattleActor battleActor)
    {
        this.battleActor = battleActor;
        this.greenBarHP = battleActor.hp;
        this.redBarHP = battleActor.hp;
        this.bActorLastHP = battleActor.hp;
        BattleManager.batman.onAnimationEnd += (ae) => { slowBarLock = false; };
    }

    private IEnumerator FastBarFunctionDMG(float start)
    {
        fastBarTimer = fastBarTime;
        float dif =  start - battleActor.hp;
        while (fastBarTimer > 0)
        {
            yield return null;
            fastBarTimer-=Time.deltaTime;
            float t = fastBarTimer/fastBarTime;
            t = fastBarCurve.Evaluate(t);
            greenBarHP = battleActor.hp+dif*t;
            greenBar.fillAmount = greenBarHP/battleActor.stats.Maxhp;
            blueBar.fillAmount = greenBarHP / battleActor.stats.Maxhp;
        }
        GreenBarRoutine = null;
        yield break;
    }
    private IEnumerator SlowBarFunctionDMG(float start)
    {
        slowBarTimer = slowBarTime;
        float dif = start - battleActor.hp;
        while (slowBarLock)
        {
            yield return null;
        }
        yield return new WaitForSeconds(BarDelay);
        while (slowBarTimer > 0)
        {
            yield return null;
            slowBarTimer -= Time.deltaTime;
            float t = slowBarTimer / slowBarTime;
            t = slowBarCurve.Evaluate(t);
            redBarHP = battleActor.hp + dif * t;
            redBar.fillAmount = redBarHP / battleActor.stats.Maxhp;
        }
        RedBarRoutine = null;
        yield break;
    }

    private IEnumerator FastBarFunctionHeal(float start) 
    {

        fastBarTimer = fastBarTime;
        float dif = start - battleActor.hp;
        while (fastBarTimer > 0)
        {
            yield return null;
            fastBarTimer -= Time.deltaTime;
            float t = fastBarTimer / fastBarTime;
            t = fastBarCurve.Evaluate(t);
            blueBarHP = battleActor.hp + dif * t;
            blueBar.fillAmount = blueBarHP / battleActor.stats.Maxhp;
            redBar.fillAmount = blueBarHP / battleActor.stats.Maxhp;
        }
        BlueBarRoutine = null;
        yield break;
    }

    private IEnumerator SlowBarFunctionHeal(float start)
    {
        slowBarTimer = slowBarTime;
        float dif = start - battleActor.hp;
        while (slowBarLock)
        {
            yield return null;
        }
        yield return new WaitForSeconds(BarDelay);
        while (slowBarTimer > 0)
        {
            yield return null;
            slowBarTimer -= Time.deltaTime;
            float t = slowBarTimer / slowBarTime;
            t = slowBarCurve.Evaluate(t);
            greenBarHP = battleActor.hp + dif * t;
            greenBar.fillAmount = greenBarHP / battleActor.stats.Maxhp;
        }
        GreenBarRoutine = null;
        yield break;
    }

    //public int BarID;
    float value;
    float max = 1;
    float percentfilled { get { return value/max; } }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (battleActor != null) 
        {
            max = battleActor.stats.Maxhp;
            value = battleActor.hp;

            if (battleActor.hp < bActorLastHP)
            {
                //Debug.LogError(fastBarRoutine);
                if (GreenBarRoutine != null)
                {
                    StopCoroutine(GreenBarRoutine);    
                }
                if (RedBarRoutine != null)
                {
                    StopCoroutine(RedBarRoutine);
                }
                // Damage
                GreenBarRoutine = StartCoroutine(FastBarFunctionDMG(greenBarHP));
                slowBarLock = true;
                RedBarRoutine = StartCoroutine(SlowBarFunctionDMG(redBarHP));
                bActorLastHP = battleActor.hp;

            }
            else if (battleActor.hp > bActorLastHP)
            {
                if (GreenBarRoutine != null)
                {
                    StopCoroutine(GreenBarRoutine);
                }
                if (BlueBarRoutine != null)
                {
                    StopCoroutine(BlueBarRoutine);
                }
                BlueBarRoutine = StartCoroutine(FastBarFunctionHeal(blueBarHP));
                slowBarLock = true;
                GreenBarRoutine = StartCoroutine(SlowBarFunctionHeal(greenBarHP));
                bActorLastHP = battleActor.hp;
            }
        }
    }
}
