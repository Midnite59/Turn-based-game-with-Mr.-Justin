using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic;
using UnityEngine.UI;
using TMPro;

public class BattleUIController : MonoBehaviour
{
    GameLoop gameloop { get { return GameLoop.instance; } }
    public Button basicButton;
    public Button skill1Button;
    public Button skill2Button;
    public List<int> targets;
    public float horizontal;
    public float vertical;
    public float deadzone;
    public int oldstatev;
    public int oldstateh;
    public int stateh;
    public int statev;


    // Start is called before the first frame update
    void Start()
    {
        //targets = new List<int>();
        //targets.Add(2);
        if (gameloop == null)
        {
            throw new System.NullReferenceException("WHERE IS GAMELOOP");
        }
        gameloop.allyTurnStart += AllyTurnStart;
        gameloop.allyTurnEnd += AllyTurnEnd;
        Debug.Log("Added");
    }
    void AllyTurnStart()
    {
        basicButton.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetBasic(), BattleManager.batman.realTargets));
        //skill1Button.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetSkill1(), targets));
        //skill2Button.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetSkill2(), targets));
    }
    void AllyTurnEnd()
    {
        basicButton.onClick.RemoveAllListeners();
        skill1Button.onClick.RemoveAllListeners();
        skill2Button.onClick.RemoveAllListeners();
    }
    void OnDestroy()
    {
        gameloop.allyTurnStart -= AllyTurnStart;
        gameloop.allyTurnEnd -= AllyTurnEnd;
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

}
