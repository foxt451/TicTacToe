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

    void OnTimedModeChange(bool isTimed)
    {
        Debug.Log(popupRectTransform.sizeDelta);
        Debug.Log(timeSectionRectTransform.sizeDelta);
        if (isTimed)
        {
            timeSection.Show();
            popupRectTransform.sizeDelta = new Vector2(popupRectTransform.sizeDelta.x,
                popupRectTransform.sizeDelta.y + timeSectionRectTransform.sizeDelta.y);
        }
        else
        {
            timeSection.Hide();
            popupRectTransform.sizeDelta = new Vector2(popupRectTransform.sizeDelta.x,
                popupRectTransform.sizeDelta.y - timeSectionRectTransform.sizeDelta.y);
        }
    }

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
            AIToggle.isOn, timeSection.GetTotalSeconds(), (0, 0), PlayerMark.Player1);

        // send message
        GameController.controller.StartNewGame(options);
    }
}
