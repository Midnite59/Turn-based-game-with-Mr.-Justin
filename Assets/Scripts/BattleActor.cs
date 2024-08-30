using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleActor : MonoBehaviour
{
    public int id;
    public Animator animator;
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
    public void HitAnimation()
    {
        BattleManager.batman.HitAnimation();
    }
    public void HurtAnimation() 
    {
        animator.SetTrigger("Hurt");
    }
}
