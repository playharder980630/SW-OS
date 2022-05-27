 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//Ä¡Æ®,UI,·©Å· µî
public class GameManager : MonoBehaviour
{
    public static GameManager Inst {get;private set;}
    void Awake() => Inst = this;

    WaitForSeconds delay2 = new WaitForSeconds(2);
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] GameObject endTurnBtn;
    // Start is called before the first frame update
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
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            if (CardManager.Inst.CheckCardCount())
            {
                CardManager.Inst.Alert(1);
                return;
            }
            CardManager.Inst.AddCard(CardManager.Inst.CheckCardCount());
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            TurnManager.Inst.EndTurn();
        }

        if (Input.GetKeyDown(KeyCode.Keypad5))
            EntityManager.Inst.DamageBoss(true, 19);

        if (Input.GetKeyDown(KeyCode.Keypad6))
            EntityManager.Inst.DamageBoss(false, 19);
    }
    public void StartGame()
    {
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
        resultPanel.Show(isMyWin ? "½Â¸®" : "ÆÐ¹è");
    }
}
