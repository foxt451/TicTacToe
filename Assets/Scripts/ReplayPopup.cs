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

    void Start()
    {
        gameObject.SetActive(true);
        replayPopupCloseButton.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        // do ui business
        gameObject.SetActive(false);

        GameOptions options = new GameOptions(timedModeToggle.isOn ? GameMode.Timed : GameMode.Difficulty, 
            AIToggle.isOn);

        // send message
        Messenger<GameOptions>.Broadcast(GameEvents.NEW_GAME, options);
    }
}
