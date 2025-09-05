using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SPIcon : MonoBehaviour
{
    public float minSP;
    public float maxSP;
    [Range(0f, 1f)]
    public float value;
    float SPdiff { get {  return maxSP - minSP; } }
    public Image img;
    public void Repaint(float glowSP, float currentSP)
    {
        value = Mathf.Clamp01((currentSP - minSP) / SPdiff);
        img.transform.localScale = Vector3.one * value;
    }
}
