using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bufficon : MonoBehaviour
{
    public Image image;
    public int hue;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StageToRGB(int stage)
    {
        int h = hue;
        float s = stage > 0 ? 1f : 0f;
        float l = stage > 0 ? (Mathf.Abs(stage) == 1 ? 0.5f : 0f) : (Mathf.Abs(stage) == 1 ? 0.5f:1f);

        float v = 1 - l;

        //other
        float c = v * s;
        float x = c * (1f - Mathf.Abs((h / 60f) % 2 - 1));
        float m = v - c;
        float r = 0f;
        float g = 0f;
        float b = 0f;
        if (0f <= h && h < 60f)
        {
            r = c + m;
            g = x + m;
            b = m;
        }
        else if (60f <= h && h < 120f)
        {
            r = x + m;
            g = c + m;
            b = m;
        }
        else if (120f <= h && h < 180f)
        {
            r = m;
            g = c + m;
            b = x + m;
        }
        else if (180f <= h && h < 240f)
        {
            r = m;
            g = x + m;
            b = c + m;
        }
        else if (240f <= h && h < 300f)
        {
            r = x + m;
            g = m;
            b = c + m;
        }
        else
        {
            r = c + m;
            g = m;
            b = x + m;
        }
        image.color = new Color(r, g, b, 1);
        if (stage == 0)
        {
            image.color = Color.white;
        }
    }

}
