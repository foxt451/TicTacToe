using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    // references to different popups
    [SerializeField]
    private ReplayPopup replayPopup;

    [SerializeField]
    private SavePopup savePopup;

    [SerializeField]
    private Button replayButton;

    [SerializeField]
    private TimedStats timedStats;

    [SerializeField]
    private EndGameInfo endInfo;

    [SerializeField]
    private Image AI_ProgressImage;

    // Start is called before the first frame update
    void Start()
    {
        InitialSetup();
    }

    private void Awake()
    {
        Messenger<GameState>.AddListener(GameEvents.GAMESTATE_CHANGED, OnGameStateChanged);
        Messenger<PlayerMark>.AddListener(GameEvents.GAME_FINISHED, OnEndGame);
        Messenger.AddListener(GameEvents.AI_START, ShowAiProgress);
        Messenger.AddListener(GameEvents.AI_FINISH, HideAiProgress);
    }

    // shows the icon with AI move processing
    private void ShowAiProgress()
    {
        AI_ProgressImage.gameObject.SetActive(true);
    }

    // hides the icon with AI move processing
    private void HideAiProgress()
    {
        AI_ProgressImage.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Messenger<GameState>.RemoveListener(GameEvents.GAMESTATE_CHANGED, OnGameStateChanged);
        Messenger<PlayerMark>.RemoveListener(GameEvents.GAME_FINISHED, OnEndGame);
        Messenger.RemoveListener(GameEvents.AI_START, ShowAiProgress);
        Messenger.RemoveListener(GameEvents.AI_FINISH, HideAiProgress);
    }

    // displays end game info
    void OnEndGame(PlayerMark playerToWin)
    {
        endInfo.DisplayEndGameInfo(playerToWin);
    }

    // opens/hides popups for the specified gameState
    void OnGameStateChanged(GameState state)
    {
        if (state == GameState.INGAME)
        {
            savePopup.Hide();
            replayPopup.Hide();
            replayButton.gameObject.SetActive(true);

            if (GameController.controller.mode == GameMode.Timed)
            {
                timedStats.Show();
            }
            else
            {
                timedStats.Hide();
            }
        }
    }

    // what UI will look at the start of the game
    void InitialSetup()
    {
        savePopup.Hide();
        replayPopup.Show(false);
        replayButton.gameObject.SetActive(false);
        timedStats.Hide();
        endInfo.Hide();
        HideAiProgress();
    }

    // opens replay popup (and closes others, if they are open)
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

    // closes replay popup
    public void CloseReplayPopup()
    {
        replayPopup.Hide();
        GameController.controller.GameState = GameState.INGAME;
    }

    // shows save popup (and closes others, if they are open)
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

    // closes save popup
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
