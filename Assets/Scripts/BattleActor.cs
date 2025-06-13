using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using BattleLogic;
using System;
using UnityEditor;
using Unity.VisualScripting;

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

    [Header(". . Torso Head RHand LHand Rfoot Lfoot .")]

    public List<Transform> focusTForms;

    [Flags]
    public enum FocusPart 
    { 
        None = 0, // 0
        Root = 1 << 0, // 1 
        Torso = 1 << 1, // 10
        Head = 1 << 2,  // 100
        RHand = 1 << 3, // 1000
        LHand = 1 << 4, // ...
        RFoot = 1 << 5, 
        LFoot = 1 << 6, 
        Target = 1 << 7,
    }

    public CharStats stats { get { return BattleManager.batman.gs.GetActor(id).stats; } }

    public bool animated { get { return animator != null && animator.enabled; } }
    public FocusPart focusPart;
    // Start is called before the first frame update
    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        targetselector.GetComponent<LookAtConstraint>().AddSource(new ConstraintSource { sourceTransform = Camera.main.transform, weight = 1f });
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void HitAnimation(int usehitanimation = 1)
    {
        BattleManager.batman.HitAnimation(usehitanimation == 1);
    }
    public void HurtAnimation(float dmg, Stance stance) 
    {
        animator.SetTrigger("Hurt");
        if (dmg >= hp)
        {
            animator.SetBool("Dead", true);
        }
        hp -= dmg;
        BattleManager.npool.Activate(Mathf.RoundToInt(dmg), stance, this);
    }
    public void HealAnimation(float dmg)
    {
        //animator.SetTrigger("Hurt");
        hp = Mathf.Min(hp + dmg, stats.Maxhp);
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
    public void LookAtBPart(int partNumber) 
    {
        FocusPart bodyPart = (FocusPart)partNumber;
    }
    public Transform BPartFilter(FocusPart bodyPart)
    {
        if (bodyPart == FocusPart.None || bodyPart == FocusPart.Target) return null;
        if (bodyPart == FocusPart.Root) return transform;
        return focusTForms[(int)Mathf.Log((int)bodyPart, 2)-1];
    }
}
