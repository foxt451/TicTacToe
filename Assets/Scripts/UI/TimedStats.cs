using UnityEngine;
using UnityEngine.UI;

// the statistics of the timed game displayed in the top part of the screen (such as time left and scores)
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

    // updates stats data according to info in gameController
    void UpdateData()
    {
        int seconds = (int)GameController.controller.totalSecondsTime;
        timeText.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");

        score1Text.text = GameController.controller.score.player1.ToString();
        score2Text.text = GameController.controller.score.player2.ToString();
    }

    // hides the section (in difficulty mode)
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // shows the section (in timed mode)
    public void Show()
    {
        UpdateData();
        gameObject.SetActive(true);
    }
}
