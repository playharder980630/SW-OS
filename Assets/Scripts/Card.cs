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
    //������ ���� �ҷ�����
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
        //���콺 �ö󰡸� ȣ��
        CardManager.Inst.CardMouseOver(this);
    }

    void OnMouseExit()
    {
        //���콺 ������Ʈ�� ������ ȣ��
        CardManager.Inst.CardMouseExit(this);
    }

    void OnMouseDown()
    {
        //���콺 Ŭ���ϸ� ȣ��
        CardManager.Inst.CardMouseDown();
    }
    void OnMouseUp()
    {
        //���콺 ī�� Ŭ������ ȣ��
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

    //Dotween�� ���� CardObject�����̱�
    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        //Dotween������
        if (useDotween)
        {
            //dotweenTime���� ī�� ��ġ,ȸ��,ũ�� ������
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        //dotween�Ⱦ��¹���
        else
        {
            //�ٷ� ��ȯ�� �����
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }
}
