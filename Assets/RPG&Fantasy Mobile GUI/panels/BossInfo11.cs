using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossInfo11 : MonoBehaviour
{
    public GameObject BossInfo; // 보스 정보
    public Button boss; // 보스 버튼

    void Start()
    {
        BossInfo.SetActive(false);

        boss.onClick.AddListener(ShowBoss);

    }

    void ShowBoss()
    {
        BossInfo.SetActive(true);

        boss.onClick.AddListener(Start);
    }
}