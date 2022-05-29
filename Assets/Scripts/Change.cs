using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Change : MonoBehaviour
{
    
    public static int StageNumberChange;
    public void SceneChangeMainToMap()
    {
        SceneManager.LoadScene("BossInfo");

    }
    public void SceneChangeMapToBattle1()
    {
        StageNumberChange = 1;
        SceneManager.LoadScene("BattleScene");

    }
    public void SceneChangeMapToBattle2()
    {
        StageNumberChange = 2;

        SceneManager.LoadScene("BattleScene");
    }
    public void SceneChangeMapToBattle3()
    {
        StageNumberChange = 3;

        SceneManager.LoadScene("BattleScene");
    }

    public void SceneChangeMapToBattle4()
    {
        StageNumberChange = 4;

        SceneManager.LoadScene("BattleScene");

    }
    public void SceneChangeMapToBattle5()
    {
        StageNumberChange = 5;

        SceneManager.LoadScene("BattleScene");

    }
    public void SceneChangeMapToBattle6()
    {
        StageNumberChange = 6;

        SceneManager.LoadScene("BattleScene");

    }
    public void SceneChangeMapToBattle7()
    {
        StageNumberChange = 7;

        SceneManager.LoadScene("BattleScene");

    }
    public void SceneChangeMapToBattle8()
    {
        StageNumberChange = 8;

        SceneManager.LoadScene("BattleScene");

    }
}