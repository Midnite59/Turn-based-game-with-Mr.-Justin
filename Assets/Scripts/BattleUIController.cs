using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic;
using UnityEngine.UI;
using TMPro;
using System;

public class BattleUIController : MonoBehaviour
{
    GameLoop gameloop { get { return GameLoop.instance; } }
    public Button basicButton;
    public Button skill1Button;
    public Button skill2Button;
    public List<int> targets;

    public List<BActorInfo> allyHealthBars;
    public float horizontal { get { return stateh; } set 
        {
            if (value > deadzone)
            {
                stateh = 1;
            }
            else if (value < -deadzone) 
            {
                stateh = -1;
            }else
            {
                stateh = 0;
            }
        }
    }
    public float vertical
    {
        get { return statev; }
        set
        {
            if (value > deadzone)
            {
                statev = 1;
            }
            else if (value < -deadzone)
            {
                statev = -1;
            }
            else
            {
                statev = 0;
            }
        }
    }
    public float deadzone = 0.001f;
    public int stateh { get { return _stateh; } set { if (value != _stateh) { _stateh = value; onStateChangeH.Invoke(value); } } }
    public int statev { get { return _statev; } set { if (value != _statev) { _statev = value; onStateChangeV.Invoke(value); } } }
    private int _stateh;
    private int _statev;
    public event Action<int> onStateChangeH = (i) => {};
    public event Action<int> onStateChangeV = (i) => {};
    private bool UIEnabled; // Ultra Instinct

    // Start is called before the first frame update
    void OnEnable()
    {
        //targets = new List<int>();
        //targets.Add(2);
        if (gameloop == null)
        {
            throw new System.NullReferenceException("WHERE IS GAMELOOP");
        }
        onStateChangeH += OnStateChangeH; // Like and Subscribe
        Debug.Log("Added");
        for (int i = 0; i < allyHealthBars.Count; i++)
        {
            if (i < BattleManager.batman.allyTeam.batactors.Count)
            {
                allyHealthBars[i].SetBattleActor(BattleManager.batman.allyTeam.batactors[i]);
            }
            else
            {
                allyHealthBars[i].gameObject.SetActive(false);
            }
        }
    }
    void AllyTurnStart()
    {
        Debug.Log("added listnars");
        basicButton.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetBasic(), BattleManager.batman.realTargets));
        skill1Button.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetSkill1(), BattleManager.batman.realTargets));
        skill2Button.onClick.AddListener(() => gameloop.TakeTurn(gameloop.currentAttr.GetSkill2(), BattleManager.batman.realTargets));

        basicButton.gameObject.SetActive(true);
        skill1Button.gameObject.SetActive(true);
        skill2Button.gameObject.SetActive(true);

        UIEnabled = true;
    }
    void AllyTurnEnd()
    {
        basicButton.onClick.RemoveAllListeners();
        skill1Button.onClick.RemoveAllListeners();
        skill2Button.onClick.RemoveAllListeners();

        basicButton.gameObject.SetActive(false);
        skill1Button.gameObject.SetActive(false);
        skill2Button.gameObject.SetActive(false);

        UIEnabled = false;
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        if (!BattleManager.batman.eventRunning && !UIEnabled)
        {
            AllyTurnStart();
        }
        else if (BattleManager.batman.eventRunning && UIEnabled)
        {
            AllyTurnEnd();
        }
    }

    void OnStateChangeH(int newState)
    {
        //we need ally targeting :O
        if (newState == 1)
        {
            BattleManager.batman.TargetChange(true);
        }
        else if (newState == -1) 
        {
            BattleManager.batman.TargetChange(false);
        }
    }

}
