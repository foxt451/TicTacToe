using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameInfo : MonoBehaviour
{
    [SerializeField]
    private Text message;

    [SerializeField]
    private Image winnerImage;
    [SerializeField]
    private Sprite player1Sprite;
    [SerializeField]
    private Sprite player2Sprite;

    private bool openedThisFrame;

    public void DisplayEndGameInfo(PlayerMark playerToWin)
    {
        if (playerToWin == PlayerMark.Empty)
        {
            winnerImage.gameObject.SetActive(false);
            message.text = "TIE...";
        }
        else
        {
            winnerImage.sprite = playerToWin == PlayerMark.Player1 ? player1Sprite : player2Sprite;
            winnerImage.gameObject.SetActive(true);
            message.text = "WINS!";
        }
        gameObject.SetActive(true);
        openedThisFrame = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (openedThisFrame)
        {
            openedThisFrame = false;
        }
        else if (Input.anyKeyDown)
        {
            if (!openedThisFrame)
            {
                Hide();
            }
        }
    }
}
