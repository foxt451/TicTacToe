using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// part of replay popup for entering the time of the game
// displayed if timed mode is selected
public class TimeSection : MonoBehaviour
{
    public int defaultMinutes = 5;

    // field to enter minutes
    [SerializeField]
    private InputField minutesField;

    // field to enter seconds
    [SerializeField]
    private InputField secondsField;

    // shows the section
    public void Show()
    {
        gameObject.SetActive(true);
    }

    // hides the section
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        minutesField.onValidateInput += ValidateMinutes;
        secondsField.onValidateInput += ValidateSeconds;
    }

    // validates minutes (only integers above or equal to 0 and below 100 are valid)
    char ValidateMinutes(string text, int charIndex, char addedChar)
    {
        string updatedText = text.Insert(charIndex, addedChar.ToString());
        if (addedChar == '-')
        {
            return '\0';
        }
        if (int.TryParse(updatedText, out int parsed))
        {
            return parsed <= 99 ? addedChar : '\0';
        }
        else
        {
            return '\0';
        }
    }

    // validates seconds (integers between 0 and 60 are valid)
    char ValidateSeconds(string text, int charIndex, char addedChar)
    {
        string updatedText = text.Insert(charIndex, addedChar.ToString());
        if (addedChar == '-')
        {
            return '\0';
        }
        if (int.TryParse(updatedText, out int parsed))
        {
            return parsed < 60 ? addedChar : '\0';
        }
        else
        {
            return '\0';
        }
    }

    // returns minutes * 60 + seconds (entered in the fields)
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
