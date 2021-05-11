using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Image replayPopup;
    [SerializeField]
    private Button replayPopupCloseButton;

    // Start is called before the first frame update
    void Start()
    {
        InitialSetup();
    }


    void InitialSetup()
    {
        // at start we have a replayPopup, but we can't close it
        replayPopup.gameObject.SetActive(true);
        replayPopupCloseButton.gameObject.SetActive(false);
        // disable the field, so that we can't move until the popup is closed
    }
}
