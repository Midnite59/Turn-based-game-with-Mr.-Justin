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
        //BattleManager.batman.SelectTargets();
        BattleManager.batman.ShowTargets(BattleManager.batman.realTargets);
        Debug.Log("ally's turn");
        basicButton.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetBasic(), targets));
        skill1Button.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetSkill1(), targets));
        skill2Button.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetSkill2(), targets));
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
}
