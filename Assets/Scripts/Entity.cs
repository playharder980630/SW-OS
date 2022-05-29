using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using DG.Tweening;
public class Entity : MonoBehaviour
{
    [SerializeField] Item item;
    [SerializeField] SpriteRenderer entity;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;
    public static Entity Inst { get; private set; }

    private void Awake() => Inst = this;
    public int attack;
    public int health;
    public bool isMine;
    public bool isDie;
    public bool isBossOrEmpty;
    public bool attackable;
    public Vector3 originPos;
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
        attack = item.attack;
        health = item.health;

        this.item = item;
        character.sprite = this.item.sprite;
        nameTMP.text = this.item.name;
        attackTMP.text = attack.ToString();
        healthTMP.text = health.ToString();
    }
    public void MoveTransform(Vector3 pos, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
            transform.DOMove(pos, dotweenTime);
        else
            transform.position = pos;
    }
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
