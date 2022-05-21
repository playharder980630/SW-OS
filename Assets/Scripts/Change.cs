using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Change : MonoBehaviour
{
    public void SceneChangeMainToMap()
    {
        SceneManager.LoadScene("BossInfo");

    }
    public void SceneChangeMapToBattle()
    {
        SceneManager.LoadScene("BattleScene");

    }
}
