using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Animations;
using static BattleCamera;
using static Unity.VisualScripting.Member;

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

    public float defaultEnemyWeight;
    public float selectedEnemyWeight;
    public float selectedAllyWeight;
    [Range(0f, 1f)]
    public float lerpSpeed;

    public Vector3 allyTargetPovOffset;

    public float selectedAllyWeightEnemyPov;

    Vector3 anchorPosition;

    public Dictionary<Transform, float> weightKeys;

    public float magna=1;


    // Start is called before the first frame update
    void Awake()
    {
        weightKeys = new Dictionary<Transform, float>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        List<ConstraintSource> sources = new List<ConstraintSource>();
        lookat.GetSources(sources);
        List<ConstraintSource> newSources = new List<ConstraintSource>();
        foreach (ConstraintSource source in sources) 
        {
            newSources.Add(new ConstraintSource { sourceTransform = source.sourceTransform, weight = Mathf.Lerp(source.weight, weightKeys[source.sourceTransform], lerpSpeed) });
        }
        lookat.SetSources(newSources);

        transform.position = Vector3.Lerp(transform.position, anchorPosition, lerpSpeed);
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
        public int id;
        public Transform transform { get { return BattleManager.batman.GetBattleActor(id).BPartFilter(fp); } }
        public float weight;
        public FocusWeight(BattleActor.FocusPart fp, float weight, int id) 
        {
            this.fp = fp;
            this.weight = weight;
            this.id = id;
        }
    }
    public void FocusAnimation(string str) 
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
                focusWeights[i] = new FocusWeight(part, weight, focusActor.id);
            }
        }
    }
    public void FocusAlly(BattleActor ally, bool force = false)
    {
        focusActor = ally;
        List<FocusWeight> focusWeights = new List<FocusWeight>();

        if (enemyTeam.batactors.Contains(BattleManager.batman.GetBattleActor(BattleManager.batman.selectTarget)))
        {
            //Vector3 enemycenter = enemyTeam.batactors.Aggregate(Vector3.zero, (total, next) => total += next.transform.position, (total) => total / enemyTeam.batactors.Count);
            Vector3 enemycenter = enemyTeam.batactors.OrderBy(a => a.transform.position.x).First().transform.position + Vector3.left * magna;
            Vector3 head = ally.headPos + Vector3.right * magna;
            Vector3 direction = (head - enemycenter).normalized;
            anchorPosition = head + direction * distanceToAlly;

            foreach (BattleActor actor in enemyTeam.batactors)
            {
                float weight = defaultEnemyWeight;
                if (actor.id == BattleManager.batman.selectTarget) weight = selectedEnemyWeight;
                focusWeights.Add(new FocusWeight(BattleActor.FocusPart.Root, weight, actor.id));
            }
            focusWeights.Add(new FocusWeight(BattleActor.FocusPart.Torso, selectedAllyWeight, ally.id));
        }
        else
        {
            Vector3 allycenter = allyTeam.batactors.Aggregate(Vector3.zero, (total, next) => total += next.transform.position, (total) => total / allyTeam.batactors.Count);
            anchorPosition = allycenter + allyTargetPovOffset;

            foreach (BattleActor actor in allyTeam.batactors)
            {
                float weight = defaultEnemyWeight;
                if (actor.id == BattleManager.batman.selectTarget) weight = selectedAllyWeightEnemyPov;
                focusWeights.Add(new FocusWeight(BattleActor.FocusPart.Root, weight, actor.id));
            }
        }
        
        if (force) 
        {
            SetLAConstraint(focusWeights.ToArray());
            transform.position = anchorPosition;
        }
        else
        {
            ChangeLAConstraint(focusWeights.ToArray());
        }


    }
    public void Focus1v1(BattleActor ally, BattleActor enemy, bool force = false)
    {
        focusActor = ally;
        List<FocusWeight> focusWeights = new List<FocusWeight>();
            Vector3 head = ally.headPos + Vector3.right * magna;
            Vector3 eHead = enemy.headPos + Vector3.left * magna;
            Vector3 direction = (head - eHead).normalized;
            anchorPosition = head + direction * distanceToAlly;

            focusWeights.Add(new FocusWeight(BattleActor.FocusPart.Root, selectedEnemyWeight, enemy.id));
            focusWeights.Add(new FocusWeight(BattleActor.FocusPart.Torso, selectedAllyWeight, ally.id));

        if (force)
        {
            SetLAConstraint(focusWeights.ToArray());
            transform.position = anchorPosition;
        }
        else
        {
            ChangeLAConstraint(focusWeights.ToArray());
        }


    }

    public void FocusEnemy(BattleActor enemy, bool force = true)
    {
        List<FocusWeight> focusWeights = new List<FocusWeight>();
        Vector3 eHead = enemy.headPos;
        anchorPosition = eHead + Vector3.back * distanceToAlly;

        focusWeights.Add(new FocusWeight(BattleActor.FocusPart.Torso, selectedEnemyWeight, enemy.id));
        focusWeights.Add(new FocusWeight(BattleActor.FocusPart.Head, selectedEnemyWeight, enemy.id));

        if (force)
        {
            SetLAConstraint(focusWeights.ToArray());
            transform.position = anchorPosition;
        }
        else
        {
            ChangeLAConstraint(focusWeights.ToArray());
        }


    }

    public void ChangeLAConstraint(params FocusWeight[] focusWeights)
    {
        List<Transform> transforms = weightKeys.Keys.ToList();
        foreach (var wk in transforms)
        {
            weightKeys[wk] = 0;
        }
        List<ConstraintSource> sources = new List<ConstraintSource>();
        lookat.GetSources(sources);
        foreach (FocusWeight focusWeight in focusWeights)
        {
            if (!sources.Any(a => a.sourceTransform == focusWeight.transform)) 
            {
                sources.Add(new ConstraintSource { sourceTransform = focusWeight.transform, weight = 0 });
            }
            weightKeys[focusWeight.transform] = focusWeight.weight;
        }
        lookat.SetSources(sources);
    }

    public void SetLAConstraint(params FocusWeight[] focusWeights)
    {
        weightKeys.Clear();
        List<ConstraintSource> sources = new List<ConstraintSource>();
        foreach (FocusWeight focusWeight in focusWeights) 
        {
            weightKeys[focusWeight.transform] = focusWeight.weight;
            sources.Add(new ConstraintSource { sourceTransform = focusWeight.transform, weight = focusWeight.weight });
        }
        lookat.SetSources(sources);
    }

}
