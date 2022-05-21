using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }

    private void Awake() => Inst = this;

    [Header("Develop")]
    [SerializeField] [Tooltip("���� �� ��带 ���մϴ�")] EturnMode eturnmode;
    [SerializeField] [Tooltip("���� ī�� ������ ���մϴ�")] int startCardCount;
    [SerializeField] [Tooltip("ī������ �ſ� �������ϴ�")] bool fastMode;
    [Header("properties")]
    public bool myTurn;
    public bool isLoading;

    enum EturnMode { Random, My, Other }
    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);
    public static Action<bool> OnAddCard;
    void GameSetUp()
    {
        if (fastMode)
            delay05 = new WaitForSeconds(0.05f);
        switch(eturnmode)
        {
            case EturnMode.Random:
                myTurn = Random.Range(0, 2) == 0;
                break;
            case EturnMode.My:
                myTurn = true;
                break;
            case EturnMode.Other:
                myTurn = false;
                break;
        }
    }
    
    public IEnumerator StartGameCo()
    {
        GameSetUp();
        isLoading = true;

        for(int i=0;i<startCardCount;i++)
        {
            yield return delay05;
            OnAddCard?.Invoke(false);
            yield return delay05;
            OnAddCard?.Invoke(true);
        }
        StartCoroutine(StartTurnCo());
    }
    public IEnumerator StartTurnCo()
    {
        isLoading = true;
        if (myTurn)
        {
            GameManager.Inst.Notification("���� ��");
        }
        yield return delay07;
        if (!CardManager.Inst.CheckCardCount())
        {
            if (myTurn)
            {
                CardManager.Inst.Alert(1);
            }
            isLoading = false;
        }
        else
        {
            CardManager.Inst.AddCard(CardManager.Inst.CheckCardCount());
            yield return delay07;
            isLoading = false;
        }
    }
    public void EndTurn()
    {
        myTurn = !myTurn;
        int getC = CardManager.Inst.myCards.Count;
        for (int i = 0; i < getC; i++)
            CardManager.Inst.SendToDiscard();
        StartCoroutine(StartTurnCo());
    }
}