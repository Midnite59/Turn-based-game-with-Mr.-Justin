using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SPIcon : MonoBehaviour
{
    public float minSP;
    public float maxSP;
    float SPdiff { get {  return maxSP - minSP; } }
    public Image img;
    void Start()
    {

    }

    public void Repaint(float sp)
    {
        img.fillAmount = Mathf.Clamp01((sp - minSP) / SPdiff);
    }
}
