using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayPopup : MonoBehaviour
{

    [SerializeField]
    private Button replayPopupCloseButton;

    void Start()
    {
        gameObject.SetActive(true);
        replayPopupCloseButton.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        // do ui business
        gameObject.SetActive(false);

        // send message
        Messenger.Broadcast(GameEvents.NEW_GAME);
    }
}
