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
    public Image gImg;
    public void Repaint(float glowSP, float currentSP)
    {
        float glowValue = Mathf.Clamp01((currentSP - minSP) / SPdiff);
        gImg.transform.localScale = Vector3.one * glowValue;
        float value = Mathf.Clamp01(((currentSP-glowSP) - minSP) / SPdiff);
        img.transform.localScale = Vector3.one * value;

    }
}
