using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthyBar : MonoBehaviour
{
    public BattleActor battleActor;
    public Image fastBar;
    public Image slowBar;
    public float slowBarTime;
    public float fastBarTime;
    public float slowBarHP;
    public float fastBarHP;
    public float bActorLastHP;
    public float slowBarDelay;

    // Timers
    float fastBarTimer;
    float slowBarTimer;

    public AnimationCurve fastBarCurve;
    public AnimationCurve slowBarCurve;

    private Coroutine fastBarRoutine;
    private Coroutine slowBarRoutine;

    private IEnumerator FastBarFunction(float start)
    {
        fastBarTimer = fastBarTime;
        float dif =  start - battleActor.hp;
        while (fastBarTimer > 0)
        {
            yield return null;
            fastBarTimer-=Time.deltaTime;
        }
        yield break;
    }
    private IEnumerator SlowBarFunction()
    {
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
                // Damage
                
            }
        }
    }
}
