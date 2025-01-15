using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleActor : MonoBehaviour
{
    public int id;
    public float hp;
    public string charname;
    public Animator animator;
    public GameObject targetselector;
    //skills
    public CharSkill basic;
    public CharSkill skill1;
    public CharSkill skill2;
    public bool animated { get { return animator != null && animator.enabled; } }
    // Start is called before the first frame update
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void HitAnimation(int usehitanimation = 1)
    {
        BattleManager.batman.HitAnimation(usehitanimation == 1);
    }
    public void HurtAnimation(float dmg) 
    {
        animator.SetTrigger("Hurt");
        if (dmg >= hp)
        {
            animator.SetBool("Dead", true);
        }
        hp -= dmg;
    }
    public void HealAnimation(float dmg)
    {
        //animator.SetTrigger("Hurt");
        hp = Mathf.Min(hp + dmg, BattleManager.batman.gs.GetActor(id).stats.Maxhp);
        if (hp > 0)
        {
            animator.SetBool("Dead", false);
            animator.ResetTrigger("Hurt");
        }

    }
    public void Target()
    {
        targetselector.SetActive(true);
    }
    public void UnTarget() {
        targetselector.SetActive(false); 
    }
}
