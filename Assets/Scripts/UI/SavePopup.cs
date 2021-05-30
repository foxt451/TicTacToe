using UnityEngine;
using UnityEngine.UI;

// popup for saving and loading games
public class SavePopup : MonoBehaviour
{
    [SerializeField]
    private Toggle[] slotToggles;
    [SerializeField]
    private Text[] toggleDateTexts;

    private void Start()
    {
        for (int i = 0; i < slotToggles.Length; i++)
        {
            UpdateDateForSlot(i);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private int GetSelectedSlot()
    {
        for (int i = 0; i < slotToggles.Length; i++)
        {
            if (slotToggles[i].isOn)
            {
                return i;
            }
        }
        return 0;
    }

    void UpdateDateForSlot(int slot)
    {
        string text;
        if (DataManager.manager.HasSavesInSlot(slot))
        {
            var dateTime = DataManager.manager.GetLastModified(slot);
            text = dateTime.ToString("dd/MM/yy HH:mm");
        }
        else
        {
            text = "empty";
        }
        toggleDateTexts[slot].text = text;
    }

    public void Save()
    {
        int selected = GetSelectedSlot();
        DataManager.manager.SaveIntoSlot(selected);
        UpdateDateForSlot(selected);
    }

    public void Load()
    {
        DataManager.manager.LoadFromSlot(GetSelectedSlot());
    }
}
