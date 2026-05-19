using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_Over_2_InfiniteModeButton : MonoBehaviour
{
    [SerializeField] private Image _myImage;
   
    [SerializeField] private Image _LockUlockImg;
    [SerializeField] private Sprite _myLockSprite;
    [SerializeField] private Sprite _myUnlockSprite;

    [SerializeField] private Image _progressFill;

    [SerializeField] private Game_Over_2_FillAmout _fill;

    private float _duration = 10f;

    private void Start()
    {
        
        if (PlayerPrefs.GetString(Constants.LEVEL_REACHED(), string.Empty).Contains(Game_Over_2_Constants.LEVEL_MAP_GAME_NAME))
        {
            _LockUlockImg.sprite = _myUnlockSprite;
            var tempColor = _myImage.color;
            tempColor.a = 1f;
            _myImage.color = tempColor;
            _LockUlockImg.color = tempColor;
        }
        else
        {
            _LockUlockImg.sprite = _myLockSprite;
            var tempColor = _myImage.color;
            tempColor.a = 0.3f;
            _myImage.color = tempColor;
            _LockUlockImg.color = tempColor;

            float levelreached = Split_List(PlayerPrefs.GetString(Game_Over_2_Constants.GAME_DATA_TOTAL_STARS));
            float target = levelreached / Game_Over_2_Manager.instance.numberOfLevel;

            _progressFill.fillAmount = target;
        }
    }

    private int Split_List(string ListLevelsReached)
    {
        if (string.IsNullOrEmpty(ListLevelsReached)) return 0;

        string[] splitedList = ListLevelsReached.Split(Game_Over_2_Constants.COOL_SEPERATOR);

        return splitedList.Length + 1;
    }
}