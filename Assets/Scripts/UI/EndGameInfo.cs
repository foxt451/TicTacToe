using UnityEngine;
using UnityEngine.UI;

// popup with end game info
public class EndGameInfo : MonoBehaviour
{
    // the text with the message about the winner
    [SerializeField]
    private Text message;

    // image, where we are going to insert the necessary player sprite of the winner
    [SerializeField]
    private Image winnerImage;
    [SerializeField]
    private Sprite player1Sprite;
    [SerializeField]
    private Sprite player2Sprite;

    // whether the popup was displayed this frame, if yes - then we don't close it even if there are some clicks
    private bool openedThisFrame;

    // display the popup
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

    // hide the popup
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
