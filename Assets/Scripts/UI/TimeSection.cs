using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSection : MonoBehaviour
{
    public int defaultMinutes = 5;

    [SerializeField]
    private InputField minutesField;

    [SerializeField]
    private InputField secondsField;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        minutesField.onValidateInput += ValidateMinutes;
        secondsField.onValidateInput += ValidateSeconds;
    }

    char ValidateMinutes(string text, int charIndex, char addedChar)
    {
        Debug.Log(text);
        if (addedChar == '-')
        {
            return '\0';
        }
        return addedChar;
    }

    char ValidateSeconds(string text, int charIndex, char addedChar)
    {
        Debug.Log(text);
        if (addedChar == '-')
        {
            return '\0';
        }
        if (int.TryParse(text + addedChar, out int parsed))
        {
            return parsed < 60 ? addedChar : '\0';
        }
        else
        {
            return '\0';
        }
    }

    public int GetTotalSeconds()
    {
        int totalSeconds = 0;
        if (int.TryParse(minutesField.text, out int minutes))
        {
            totalSeconds += minutes * 60;
        }
        else
        {
            totalSeconds += defaultMinutes * 60;
        }

        if (int.TryParse(secondsField.text, out int seconds))
        {
            totalSeconds += seconds;
        }
        else
        {
            totalSeconds += 0;
        }

        return totalSeconds;
    }
}
