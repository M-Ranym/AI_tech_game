using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using NoSuchStudio.Localization;

[RequireComponent(typeof(Toggle))]
public class LanguageToggle : MonoBehaviour
{
    [SerializeField][LocaleProperty(true)] private string _locale;

    void Start() {
        if (GetComponent<Toggle>().isOn) {
            OnToggleClick(true);
        }
    }

    public void OnToggleClick(bool b) {
        if (b) {
            LocalizationService.CurrentLocale = _locale;
        }
    }
}
