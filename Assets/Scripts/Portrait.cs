using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(RectTransform), typeof(RawImage))]
public class Portrait : MonoBehaviour
{
    RectTransform rectTransform;
    RawImage rawImage;
    Vector2 ogDimensions;
    Vector2 shrunkDimensions { get { return ogDimensions * shrinkFactor; } }
    [Range(0, 1)]
    public float shrinkFactor = 0.5f;
    [Range(0, 1)]
    public float lerpT = 0.5f;

    public BattleActor battleActor;

    Vector2 tv2; // rectTransform.sizeDelta
    Color tc; // rawImage.color

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ogDimensions = rectTransform.sizeDelta;
        rawImage = GetComponent<RawImage>();
        rawImage.color = Color.black;
        rectTransform.sizeDelta = shrunkDimensions;
        tc = rawImage.color;
        tv2 = rectTransform.sizeDelta;

    }

    // Fixed Update is called once per frame but it is fixed.
    void FixedUpdate()
    {
        if (battleActor != null && BattleManager.curActorID == battleActor.id) 
        {
            Activate();
        }
        else
        {
            Deactivate();
        }

        rawImage.color = Color.Lerp(rawImage.color, tc, lerpT);
        rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, tv2, lerpT);
    }

    public void Activate()
    {
        //turn size normal
        //fix colors
        tc = Color.white;
        tv2 = ogDimensions;

    }

    public void Deactivate()
    {
        //turn black
        //shrink
        tc = Color.black;
        tv2 = shrunkDimensions;

    }
}
