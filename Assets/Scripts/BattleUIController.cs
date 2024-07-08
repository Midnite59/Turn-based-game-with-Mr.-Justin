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

    // Start is called before the first frame update
    void Start()
    {
        gameloop.allyTurnStart += AllyTurnStart;
        gameloop.allyTurnEnd += AllyTurnEnd;
        Debug.Log("Added");
    }
    void AllyTurnStart()
    {
        Debug.Log("ally's turn");
        basicButton.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetBasic(), gameloop.currentTargets));
        skill1Button.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetSkill1(), gameloop.currentTargets));
        skill2Button.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetSkill2(), gameloop.currentTargets));
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
