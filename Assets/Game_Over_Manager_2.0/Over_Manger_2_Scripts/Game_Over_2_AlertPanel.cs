using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UPersian.Components;

public class Game_Over_2_AlertPanel : MonoBehaviour
{
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private GameObject _contentPanel;

    [SerializeField] private GameObject _internetDownPanel;
    [SerializeField] private RtlText _internetDownTxt;

    private Game_Over_2_Manager _gameOverManager;

    private void Start()
    {
        _gameOverManager = Game_Over_2_Manager.instance;
    }

    public void Loading(bool isActive)
    {
        _loadingPanel.SetActive(isActive);
    }

    public void Content(bool isActive)
    {
        _contentPanel.SetActive(isActive);
    }

    public void Internet_Down(bool isActive)
    {
        _internetDownPanel.SetActive(isActive);

        _internetDownTxt.enabled = true;
        _internetDownTxt.text = Internet_Down_Text(_gameOverManager.Return_Language());
    }

    private string Internet_Down_Text(Language lan)
    {
        string text = "";
        switch (lan)
        {
            case Language.EN:
                text = Game_Over_2_Constants.INTERNET_DOWN_TEXT_EN;
                break;

            case Language.AR:
                text = Game_Over_2_Constants.INTERNET_DOWN_TEXT_AR;
                break;

            case Language.FR:
                text = Game_Over_2_Constants.INTERNET_DOWN_TEXT_Fr;
                break;
        }

        return text;
    }
}