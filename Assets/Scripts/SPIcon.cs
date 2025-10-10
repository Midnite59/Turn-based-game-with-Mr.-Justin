using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SPIcon : MonoBehaviour
{
    public float minSP;
    public float maxSP;

    public Color glow1;
    public Color glow2;

    public float gTime;

    public AnimationCurve acurve;

    private float timer;

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
    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= gTime) {
            timer -= gTime;
        }
        gImg.color = Color.Lerp(glow1,glow2,acurve.Evaluate(timer / (gTime * 0.5f)));
    }
}
