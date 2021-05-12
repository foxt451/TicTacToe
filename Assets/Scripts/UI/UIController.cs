using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private ReplayPopup replayPopup;

    [SerializeField]
    private SavePopup savePopup;

    // Start is called before the first frame update
    void Start()
    {
        InitialSetup();
    }


    void InitialSetup()
    {
        replayPopup.Show(false);
    }

    public void ShowSavePopup()
    {
        savePopup.Show();
        if (GameController.controller.gameState == GameState.INGAME)
        {
            GameController.controller.gameState = GameState.PAUSED;
        }
        else
        {
            replayPopup.Hide();
        }
    }

    public void CloseSavePopup()
    {
        savePopup.Hide();
        if (GameController.controller.gameState == GameState.PREGAME)
        {
            replayPopup.Show(false);
        }
        else
        {
            GameController.controller.gameState = GameState.INGAME;
        }
    }
}
