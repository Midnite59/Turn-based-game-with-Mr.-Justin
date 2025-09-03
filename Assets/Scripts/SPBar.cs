using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPBar : MonoBehaviour
{
    public int maxSP;
    public SPIcon iconPF;
    public float skillPoints { get { return BattleManager.batman.gs.allyStancePoints; } }
    public List<SPIcon> sPIcons;

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
        Repaint();
    }
    public void Repaint()
    {
        foreach (SPIcon icon in sPIcons) 
        {
            icon.Repaint(skillPoints);
        }
    }
}
