using UnityEngine;
using UnityEngine.UI;

// popup for starting a new game and selecting its options
public class ReplayPopup : MonoBehaviour
{

    [SerializeField]
    private Button replayPopupCloseButton;

    [SerializeField]
    private Toggle timedModeToggle;
    [SerializeField]
    private Toggle AIToggle;

    [SerializeField]
    private TimeSection timeSection;

    private RectTransform popupRectTransform;
    private RectTransform timeSectionRectTransform;

    private void Start()
    {
        timedModeToggle.onValueChanged.AddListener(OnTimedModeChange);

        popupRectTransform = GetComponent<RectTransform>();
        timeSectionRectTransform = timeSection.GetComponent<RectTransform>();

        OnTimedModeChange(timedModeToggle.isOn);
    }

    // updates the popup if we select different mode (opens/closes TimeSection)
    void OnTimedModeChange(bool isTimed)
    {
        if (isTimed)
        {
            if (!timeSection.gameObject.activeSelf)
            {
                timeSection.Show();
                popupRectTransform.sizeDelta = new Vector2(popupRectTransform.sizeDelta.x,
                    popupRectTransform.sizeDelta.y + timeSectionRectTransform.sizeDelta.y);
            }
        }
        else
        {
            timeSection.Hide();
            popupRectTransform.sizeDelta = new Vector2(popupRectTransform.sizeDelta.x,
                popupRectTransform.sizeDelta.y - timeSectionRectTransform.sizeDelta.y);
        }
    }
    
    // opens the popup
    public void Show(bool canClose)
    {
        gameObject.SetActive(true);
        replayPopupCloseButton.gameObject.SetActive(canClose);
    }

    // hides the popup
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // tells the gameController to start a new game
    public void StartGame()
    {
        Hide();

        GameOptions options = new GameOptions(timedModeToggle.isOn ? GameMode.Timed : GameMode.Difficulty, 
            AIToggle.isOn, timeSection.GetTotalSeconds(), (0, 0), PlayerMark.Player1, false);

        // send message
        GameController.controller.StartNewGame(MousePanner.defaultCameraPos, options);
    }
}
