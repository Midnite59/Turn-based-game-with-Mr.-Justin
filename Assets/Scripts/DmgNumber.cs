using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DmgNumber : MonoBehaviour
{
    public TMP_Text text;
    float lifetime;
    public float popin = 0.2f;
    public float stay = 1f;
    public float fade = 0.3f;
    public AnimationCurve fadeCurve;
    float finalScaleMag;
    public float startingScaleMultiplier;
    public AnimationCurve scaleCurve;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (lifetime >= 0 && lifetime <= popin)
        {
            float startingScaleMag = finalScaleMag * startingScaleMultiplier;
            transform.localScale = Vector3.one * (finalScaleMag + (startingScaleMag - finalScaleMag) * scaleCurve.Evaluate(lifetime / popin));
            Debug.Log("Popin");
            //text.color = new Color(text.color.r, text.color.g, text.color.b, fadeCurve.Evaluate(lifetime / popin));
        }
        else if (lifetime <= popin+stay)
        {
            Debug.Log("Stay");
        }
        else if (lifetime <= popin+stay+fade)
        {
            float startOp = lifetime-(popin+stay);
            text.color = new Color(text.color.r, text.color.g, text.color.b, fadeCurve.Evaluate(startOp / fade));
            Debug.Log("Fade");
        }
        else if (lifetime > popin + stay + fade)
        {
            //Debug.Log("destroy " + lifetime);
            Deactivate();
        }
        lifetime += Time.deltaTime;
        

    }

    private void OnEnable()
    {
        
    }

    public void Setup(int number, BattleLogic.Stance stance, BattleActor battleActor)
    {
        transform.position = battleActor.transform.position + Vector3.up * 2;
        transform.position += Random.onUnitSphere * Random.Range(1, 1.5f) * 0.7f;
        transform.position += (Camera.main.transform.position - transform.position).normalized * Mathf.Sign(Random.Range(-1, 1)) * 0.75f;
        finalScaleMag = Vector3.Distance(transform.position, Camera.main.transform.position) * 0.05f;
        transform.rotation = Camera.main.transform.rotation;
        text.text = number.ToString();
        text.color = BattleManager.sglt.GetColor(stance);
        lifetime = 0;
        gameObject.SetActive(true);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
