using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavePopup : MonoBehaviour
{
    [SerializeField]
    private Toggle[] slotToggles;

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
                return i + 1;
            }
        }
        return 0;
    }

    public void Save()
    {
        DataManager.manager.SaveIntoSlot(GetSelectedSlot());
    }

    public void Load()
    {
        DataManager.manager.LoadFromSlot(GetSelectedSlot());
    }
}
