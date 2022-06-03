using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [SerializeField] SpriteRenderer card;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] Sprite cardFront;
    [SerializeField] Sprite cardBack;
    [SerializeField] int Class;
    public Item item;
    public PRS originPRS;
    //아이템 정보 불러오기
    public void Setup(Item item)
    {
        this.item = item;
        character.sprite = this.item.sprite;
        nameTMP.text = this.item.name;
        attackTMP.text = this.item.attack.ToString();
        healthTMP.text = this.item.health.ToString();
        Class = this.item.Class;
    }

    void OnMouseOver()
    {
        //마우스 올라가면 호출
        CardManager.Inst.CardMouseOver(this);
    }

    void OnMouseExit()
    {
        //마우스 오브젝트에 나가면 호출
        CardManager.Inst.CardMouseExit(this);
    }

    void OnMouseDown()
    {
        //마우스 클릭하면 호출
        CardManager.Inst.CardMouseDown();
    }
    void OnMouseUp()
    {
        //마우스 카드 클릭떼면 호출
        CardManager.Inst.CardMouseUp();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //Dotween을 통해 CardObject움직이기
    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        //Dotween사용버전
        if (useDotween)
        {
            //dotweenTime동안 카드 위치,회전,크기 조정됨
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        //dotween안쓰는버전
        else
        {
            //바로 변환이 적용됨
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }
}
