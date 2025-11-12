using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPBar : MonoBehaviour
{
    public int maxSP;
    public SPIcon iconPF;
    float skillPoints;
    public List<SPIcon> sPIcons;
    public float currentSP;
    float glowSP;

    [Range(0f, 1f)]
    public float lerpT;

    public void UpdateSP(float sp)
    {
        skillPoints = sp;
    }

    void Awake()
    {
        sPIcons = new List<SPIcon>();
        for (int i = 0; i < maxSP; i++)
        {
            SPIcon pf = Instantiate(iconPF, transform);
            sPIcons.Add(pf);
            pf.maxSP = i+1;
            pf.minSP = i;
        }
    }
    private void Update()
    {
        glowSP = Mathf.Lerp(glowSP, BattleManager.batman.selectedSkill != null ? BattleManager.batman.selectedSkill.cost : 0f, lerpT); 
        currentSP = Mathf.Lerp(currentSP, skillPoints, lerpT);
        Repaint();
    }
    public void Repaint()
    {
        
        foreach (SPIcon icon in sPIcons) 
        {
            icon.Repaint(glowSP, currentSP);
        }
    }
}
