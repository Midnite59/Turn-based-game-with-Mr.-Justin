using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NumberPool : MonoBehaviour
{
    public List<DmgNumber> dmgNumbers;
    public DmgNumber numberPrefab;
    public int poolMax = 5;

    // Start is called before the first frame update
    // Numberpool & Stringerine
    void Awake()
    {
        for (int i = 0; i < poolMax; i++) 
        {
            DmgNumber numInst = Instantiate(numberPrefab);
            numInst.gameObject.SetActive(false);
            numInst.name = string.Format("Damage Number {0}", i+1);
            numInst.transform.SetParent(transform);
            dmgNumbers.Add(numInst);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Activate(int number, BattleLogic.Stance stance, BattleActor battleActor)
    {
        DmgNumber availNumber = dmgNumbers.FirstOrDefault(n => !n.gameObject.activeSelf);
        if (availNumber == null)
        {
            Debug.LogError("HELP I RAN OUTTA NUMBERS");
            return;
        }
        else
        {
            availNumber.Setup(number, stance, battleActor);
        }
    }
}
