using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimedStats : MonoBehaviour
{
    [SerializeField]
    private Text score1Text;

    [SerializeField]
    private Text score2Text;

    [SerializeField]
    private Text timeText;

    void Update()
    {
        UpdateData();
    }

    void UpdateData()
    {
        int seconds = (int)GameController.controller.totalSecondsTime;
        timeText.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");

        score1Text.text = GameController.controller.score.player1.ToString();
        score2Text.text = GameController.controller.score.player2.ToString();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        UpdateData();
        gameObject.SetActive(true);
    }
}
