using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using System.Text.RegularExpressions;

[RequireComponent(typeof(LookAtConstraint))]
public class BattleCamera : MonoBehaviour
{
    public BattleTeam allyTeam;
    public BattleTeam enemyTeam;

    public BattleActor focusActor;

    public List<Transform> dynamicTransforms;
    public LookAtConstraint lookat;

    public float distanceToAlly;
    public Vector3 offset;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void DynamicTransform() 
    {
        int index = 0;
        for (int i = 0; i < allyTeam.transforms.Count; i++) 
        {
            BattleActor bactor = allyTeam.transforms[i].GetComponentInChildren<BattleActor>(true);
            if (bactor != null)
            {
                bactor.transform.SetParent(dynamicTransforms[index]);
                bactor.transform.localPosition = Vector3.zero;
                bactor.transform.localRotation = Quaternion.identity;
                bactor.transform.localScale = Vector3.one;
            }
            index++;
        }
        // What are you gonna do about it?
        foreach (int i in Enumerable.Range(0, enemyTeam.transforms.Count)) 
        {
            BattleActor bactor = enemyTeam.transforms[i].GetComponentInChildren<BattleActor>(true);
            if (bactor != null)
            {
                bactor.transform.SetParent(dynamicTransforms[index]);
                bactor.transform.localPosition = Vector3.zero;
                bactor.transform.localRotation = Quaternion.identity;
                bactor.transform.localScale = Vector3.one;
            }
            index++;
        }
    }
    public void StandardTransform() 
    {
        int index = 0;
        foreach (int i in Enumerable.Range(0, allyTeam.transforms.Count))
        {
            BattleActor bactor = dynamicTransforms[index].GetComponentInChildren<BattleActor>(true);
            if (bactor != null)
            {
                bactor.transform.SetParent(allyTeam.transforms[i]);
                bactor.transform.localPosition = Vector3.zero;
                bactor.transform.localRotation = Quaternion.identity;
                bactor.transform.localScale = Vector3.one;
            }
            index++;
        }
        // What are you gonna do about it?
        for (int i = 0; i < enemyTeam.transforms.Count; i++)
        {
            BattleActor bactor = dynamicTransforms[index].GetComponentInChildren<BattleActor>(true);
            if (bactor != null)
            {
                bactor.transform.SetParent(enemyTeam.transforms[i]);
                bactor.transform.localPosition = Vector3.zero;
                bactor.transform.localRotation = Quaternion.identity;
                bactor.transform.localScale = Vector3.one;
            }
            index++;
        }
    }
    public class FocusWeight
    {
        public BattleActor.FocusPart fp;
        public float weight;
        public FocusWeight(BattleActor.FocusPart fp, float weight) 
        {
            this.fp = fp;
            this.weight = weight;
        }
    }
    public void LookAt(string str) 
    {
        string[] strings = str.Split(';');
        FocusWeight[] focusWeights = new FocusWeight[strings.Length];
        Regex pat = new Regex("[\\d.]");
        for (int i = 0; i < strings.Length; i++)
        {
            string s = strings[i];
            Match mat = pat.Match(s);
            if (!mat.Success)
            {
                Debug.LogError("malformed string dumdum");
            }
            else 
            {
                string partStr = s.Substring(0, mat.Index);
                string weightStr = s.Substring(mat.Index);
                //print(part);
                //print(weight);
                BattleActor.FocusPart part = (BattleActor.FocusPart)System.Enum.Parse(typeof(BattleActor.FocusPart), partStr);
                float weight = float.Parse(weightStr);
                focusWeights[i] = new FocusWeight(part, weight);
            }
        }
    }
    public void FocusAlly(BattleActor ally)
    {
        Vector3 enemycenter = enemyTeam.batactors.Aggregate(Vector3.zero, (total, next) => total += next.transform.position, (total) => total / enemyTeam.batactors.Count);
        Vector3 head = ally.BPartFilter(BattleActor.FocusPart.Head).position;
        Vector3 direction = (head - enemycenter).normalized;
        transform.position = head + direction * distanceToAlly + offset;
        ChangeLAConstraint(enemyTeam.batactors.Select(ba => ba.transform).Append(ally.transform).ToArray());
    }
    public void ChangeLAConstraint(params Transform[] transforms)
    {
        List<ConstraintSource> sources = new List<ConstraintSource>();
        foreach (Transform tform in transforms) 
        {
            sources.Add(new ConstraintSource { sourceTransform = tform, weight = 1 });
        }
        lookat.SetSources(sources);
    }

}
