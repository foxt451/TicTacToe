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

    [SerializeField]
    private Button replayButton;

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
            replayButton.gameObject.SetActive(true);
        }
    }


    void InitialSetup()
    {
        savePopup.Hide();
        replayPopup.Show(false);
        replayButton.gameObject.SetActive(false);
    }

    public void ShowReplayPopup()
    {
        savePopup.Hide();
        if (GameController.controller.GameState == GameState.PREGAME)
        {
            replayPopup.Show(false);
        }
        else
        {
            replayPopup.Show(true);
            GameController.controller.GameState = GameState.PAUSED;
        }
    }

    public void CloseReplayPopup()
    {
        replayPopup.Hide();
        GameController.controller.GameState = GameState.INGAME;
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
