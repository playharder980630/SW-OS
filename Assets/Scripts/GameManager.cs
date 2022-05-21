 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//Ä¡Æ®,UI,·©Å· µî
public class GameManager : MonoBehaviour
{
    public static GameManager Inst {get;private set;}
    void Awake() => Inst = this;

    [SerializeField] NotificationPanel notificationPanel;
    
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
    }
    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }
    public void Notification(string message)
    {
        notificationPanel.Show(message);
    }
}
