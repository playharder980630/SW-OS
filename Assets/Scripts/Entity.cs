using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using DG.Tweening;
public class Entity : MonoBehaviour
{
    // Entity 정보들 유니티에서 SerializeField 해서 넣어주기 
    [SerializeField] Item item;
    [SerializeField] SpriteRenderer entity;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameTMP; 
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;                    
    public static Entity Inst { get; private set; }
    // bool형 변수들로 제어하기 위한 변수들 선언
    private void Awake() => Inst = this;
    public int attack;
    public int health;
    public bool isMine;
    public bool isDie;
    public bool isBossOrEmpty;
    public bool attackable;
    public int Class;
    public Vector3 originPos; // 카드 정렬을 표현하기위한 Vector
    int liveCount;
    void Start()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;
    }

    void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }
    public void Setup(Item item)
    {
    // 카드 정보에 따라 카드 초기값 Setup
        attack = item.health;
        health = item.attack;

        this.item = item;
        character.sprite = this.item.sprite;
        nameTMP.text = this.item.name;
        attackTMP.text = attack.ToString();
        healthTMP.text = health.ToString();
        Class = this.item.Class;
    }
    public void MoveTransform(Vector3 pos, bool useDotween, float dotweenTime = 0)
    {
    //카드를 정렬하기위한 함수 Dotween을 이용해서 pos로 이동
        if (useDotween)
            transform.DOMove(pos, dotweenTime);
        else
            transform.position = pos;
    }
    // 마우스 다운 , 업 , 드래그시 엔티티 매니저에서 해당 함수를 호출해 작동
    void OnMouseDown()
    {
        if (isMine)
            EntityManager.Inst.EntityMouseDown(this);
    }

    void OnMouseUp()
    {
        if (isMine)
            EntityManager.Inst.EntityMouseUp();
    }

    void OnMouseDrag()
    {
        if (isMine)
            EntityManager.Inst.EntityMouseDrag();
    }
    void OnTurnStarted(bool myTurn)
    {
        if (isBossOrEmpty)
            return;

        if (isMine == myTurn)
            liveCount++;
        
    }
    public bool Damaged(int damage)
    {
    // 어택시 damage를 계산해주고 체력이 0보다 작아지면 isDie를 true로 보내 죽음 처리 할 수 있게 제어
        health -= damage;
        healthTMP.text = health.ToString();

        if (health <= 0)
        {
            isDie = true;
            return true;
        }
        return false;
    }
    public bool BossUpdate(int damage,int stagenumber)
    {
    // 보스의 체력 정보 이름등의 정보를 Stagenumber에 따라 업데이트
    // 특정 카드효과에 따라 damage를 줌 ->미구현
        health = 10 + 10 * stagenumber;
        attack = 1 + 1 * stagenumber;
        healthTMP.text = health.ToString();
        attackTMP.text = attack.ToString();
        if (stagenumber == 1)
            nameTMP.text = "고블린";
        else if (stagenumber == 2)
            nameTMP.text = "오크";
        else if (stagenumber == 3)
            nameTMP.text = "오우거";
        else if (stagenumber == 4)
            nameTMP.text = "트롤";
        else if (stagenumber == 5)
            nameTMP.text = "험상궂은손님";
        else if (stagenumber == 6)
            nameTMP.text = "나가";
        else if (stagenumber == 7)
            nameTMP.text = "드래곤";
        else if (stagenumber == 8)
            nameTMP.text = "광전사";
        Sprite[] sprites1 = Resources.LoadAll<Sprite>("Sprites");
        character.sprite = sprites1[stagenumber-1];
        if (health <= 0)
        {
            isDie = true;
            return true;
        }
        return false;
    }
}
