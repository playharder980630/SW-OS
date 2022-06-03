using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//ġƮ,UI,��ŷ ��
public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    WaitForSeconds delay2 = new WaitForSeconds(2);
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] GameObject endTurnBtn;
    // Start is called before the first frame update

    //���� ���� �ϱ�
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
        //1�� ������
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            //��ο� ���� ī�� ���� Ȯ���ؼ�
            if (CardManager.Inst.CheckCardCount())
            {
                //ī�� �� �� ������ ���� ȣ��
                CardManager.Inst.Alert(1);
                return;
            }
            //ī�� ��ο��ϱ�
            CardManager.Inst.AddCard(CardManager.Inst.CheckCardCount());
        }
        //2�� ������
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            //�������ϱ�
            TurnManager.Inst.EndTurn();
        }
        //5�� ������
        if (Input.GetKeyDown(KeyCode.Keypad5))
            //������ 19������ �ֱ�
            EntityManager.Inst.DamageBoss(true, 19);
        //6�� ������
        if (Input.GetKeyDown(KeyCode.Keypad6))
            //��뿡�� 19������ �ֱ�
            EntityManager.Inst.DamageBoss(false, 19);
    }
    public void StartGame()
    {
        //TurnManager���� STartGameCoȣ��
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
        resultPanel.Show(isMyWin ? "�¸�" : "�й�");
    }
}
