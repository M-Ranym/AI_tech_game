using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class Game_Over_2_Web_fetchBehavior : MonoBehaviour
{
    private bool _fetchKey = false;
    private bool _keyExists = false;

    private string _keyTxt;
    private int _count = 0;

    private string _result;

    

    [SerializeField] private GameObject _fetchPanel;
    [SerializeField] private RtlText _text;

    private const int MARGEOFATTEMPT = 5;

    private void Start()
    {
        if (_text != null)
        _text.gameObject.SetActive(false);
    }

    public void Fetch_Panel(bool isOpened)
    {
        _fetchPanel.SetActive(isOpened);
    }

    public void Fetch_Key(string key)
    {
        Fetch_Panel(true);
		_keyTxt = key;
        _fetchKey = true;
        _count = 0;
    }

    private async void Update()
    {
        if (_fetchKey)
        {
            _fetchKey = false;
            Debug.Log(_keyTxt);
            
        }

        if (_keyExists)
        {
            _keyExists = false;
            _result = _result.Replace("\"", string.Empty);
            string[] userInfo = Split_String(_result.Replace("\"", string.Empty));
          
        }
    }

    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    

    private string[] Split_String(string s)
    {
        return s.Split(Game_Over_2_Constants.WEC_SEPERATOR, System.StringSplitOptions.RemoveEmptyEntries);
    }

    private void Generate_Key()
    {
        string[] keyTab = Split_String(_keyTxt);

        long x = long.Parse(keyTab[0]);

        x--;

        keyTab[0] = x.ToString();

        _keyTxt = string.Join(Game_Over_2_Constants.WEC_SEPERATOR[0], keyTab);

        if (_count > MARGEOFATTEMPT)
        {
            _text.text = Refresh_Page_Message(Game_Over_2_Manager.instance.Return_Language());
        }
        else
        {
            _fetchKey = true;
            _count++;
        }
    }

    private string Refresh_Page_Message(Language lan)
    {
        string txt = "";
        switch (lan)
        {
            case Language.AR:
                txt = "قم بتحديث صفحتك";
                break;

            case Language.EN:
                txt = "Refresh your page";
                break;
        }
        _text.gameObject.SetActive(true);
        return txt;
        
    }
}