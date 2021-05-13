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

    private void Awake()
    {
        Messenger<GameState>.AddListener(GameEvents.GAMESTATE_CHANGED, OnGameStateChanged);
    }

    private void OnDestroy()
    {
        Messenger<GameState>.RemoveListener(GameEvents.GAMESTATE_CHANGED, OnGameStateChanged);
    }

    void OnGameStateChanged(GameState state)
    {
        if (state == GameState.INGAME)
        {
            savePopup.Hide();
            replayPopup.Hide();
        }
    }


    void InitialSetup()
    {
        replayPopup.Show(false);
    }

    public void ShowSavePopup()
    {
        savePopup.Show();
        if (GameController.controller.GameState == GameState.INGAME)
        {
            GameController.controller.GameState = GameState.PAUSED;
        }
        else
        {
            replayPopup.Hide();
        }
    }

    public void CloseSavePopup()
    {
        savePopup.Hide();
        if (GameController.controller.GameState == GameState.PREGAME)
        {
            replayPopup.Show(false);
        }
        else if (GameController.controller.GameState == GameState.PAUSED)
        {
            GameController.controller.GameState = GameState.INGAME;
        }
    }
}
