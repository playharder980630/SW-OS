using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//치트,UI,랭킹 등
public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    WaitForSeconds delay2 = new WaitForSeconds(2);
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] GameObject endTurnBtn;
    // Start is called before the first frame update

    //게임 시작 하기
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        InputCheatKey();
#endif
    }
    void InputCheatKey()
    {
        //1번 누르면
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            //드로우 전에 카드 개수 확인해서
            if (CardManager.Inst.CheckCardCount())
            {
                //카드 다 차 있으면 오류 호출
                CardManager.Inst.Alert(1);
                return;
            }
            //카드 드로우하기
            CardManager.Inst.AddCard(CardManager.Inst.CheckCardCount());
        }
        //2번 누르면
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            //턴종료하기
            TurnManager.Inst.EndTurn();
        }
        //5번 누르면
        if (Input.GetKeyDown(KeyCode.Keypad5))
            //나에게 19데미지 주기
            EntityManager.Inst.DamageBoss(true, 19);
        //6번 누르면
        if (Input.GetKeyDown(KeyCode.Keypad6))
            //상대에게 19데미지 주기
            EntityManager.Inst.DamageBoss(false, 19);
    }
    public void StartGame()
    {
        //TurnManager에서 STartGameCo호출
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }
    public void Notification(string message)
    {
        notificationPanel.Show(message);
    }
    public IEnumerator GameOver(bool isMyWin)
    {
        TurnManager.Inst.isLoading = true;
        endTurnBtn.SetActive(false);
        yield return delay2;

        TurnManager.Inst.isLoading = true;
        resultPanel.Show(isMyWin ? "승리" : "패배");
    }
}
