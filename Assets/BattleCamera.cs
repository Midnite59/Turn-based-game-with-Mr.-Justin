using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleCamera : MonoBehaviour
{
    public BattleTeam allyTeam;
    public BattleTeam enemyTeam;

    public List<Transform> dynamicTransforms;

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
    public void LookAtTform(Transform transform) 
    {

    }

}
