using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] TMP_Text resultTMP;


    public void Show(string message)
    {
        if (Change.StageNumberChange == 8 && message == "�¸�")
        {
            SceneManager.LoadScene("FinalWin");

        }
        else
        {
            resultTMP.text = message;
            transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);
            SceneManager.LoadScene("RewardScene");
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    void Start() => ScaleZero();

    [ContextMenu("ScaleOne")]
    void ScaleOne() => transform.localScale = Vector3.one;

    [ContextMenu("ScaleZero")]
    public void ScaleZero() => transform.localScale = Vector3.zero;
}
