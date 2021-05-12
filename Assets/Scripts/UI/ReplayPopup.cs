using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayPopup : MonoBehaviour
{

    [SerializeField]
    private Button replayPopupCloseButton;

    [SerializeField]
    private Toggle timedModeToggle;
    [SerializeField]
    private Toggle AIToggle;

    public void Show(bool canClose)
    {
        gameObject.SetActive(true);
        replayPopupCloseButton.gameObject.SetActive(canClose);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void StartGame()
    {
        Hide();

        GameOptions options = new GameOptions(timedModeToggle.isOn ? GameMode.Timed : GameMode.Difficulty, 
            AIToggle.isOn, 15, (0, 0));

        // send message
        GameController.controller.StartNewGame(options);
    }
}
