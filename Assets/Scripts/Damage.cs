using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Damage : MonoBehaviour
{
    [SerializeField] TMP_Text damageTMP; //damageTMP 를 넣어주기
    Transform tr;

    public void SetupTransform(Transform tr)
    {
        this.tr = tr;
    }

    void Update()
    {
        if (tr != null)
            transform.position = tr.position; // 현재tr entity가 죽었을 때 업데이트
    }

    public void Damaged(int damage)
    {
        if (damage <= 0) 
            return;

        GetComponent<Order>().SetOrder(1000); //Order스크립트상 가장 앞에 두기 위해 
        damageTMP.text = $"-{damage}";

        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one * 1.8f, 0.5f).SetEase(Ease.InOutBack)) // 데미지 Object 크기 크게하는 효과
            .AppendInterval(1.2f)
            .Append(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack)) // 데미지 Object 크기 작게해서 울렁이는 효과 주기
            .OnComplete(() => Destroy(gameObject)); 
    }
}
