using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UPersian.Components;
using com.horizon.LocalizationSystem;

public class Game_Over_2_Node : MonoBehaviour
{
    [SerializeField] private RtlText _levelIndexTxt;
    [SerializeField] private Image _backGroundChunk;

    [HideInInspector] public int levelIndex;

    [SerializeField] private GameObject[] _stars;
    [SerializeField] private GameObject[] _unfilledStars;
    [SerializeField] private GameObject _levelIndexGameObject;
    [SerializeField] private GameObject _lockIcon;
    [SerializeField] private Image _myImg;
    [SerializeField] private Sprite _lockedSprite;

    [SerializeField] private GameObject _marker;

    public bool isLevelReached;
    [Space]
    [Header("Localization")]
    //--localization
    [SerializeField] private Transform RtlPositionTr;
    [SerializeField] private Transform LtrPositionTr;
    //----

    private void Start()
    {
        Reached_Level_Stars_Counting_Behaviour();
    }

    public void Set_Me(int levelIndex, Sprite sprite)
    {
        this.levelIndex = levelIndex;
        this._levelIndexTxt.text = levelIndex.ToString();
        this._backGroundChunk.sprite = sprite;
    }

    /*Change Stars Sprites*/

    private void Reached_Level_Stars_Counting_Behaviour()
    {
        if (PlayerPrefs.HasKey(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + levelIndex))
        {
            Debug.Log(Game_Over_2_Constants.GAME_DATA_STARS);
            for (int i = 0; i < PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + levelIndex); i++)
            {
                _stars[i].SetActive(true);
            }
            isLevelReached = true;
        }
        else if (PlayerPrefs.HasKey(Game_Over_2_Constants.GAME_DATA_STARS + Game_Over_2_Constants.COOL_SEPERATOR + (levelIndex - 1)))
        {
            // ------ localization
            bool success = false;
            if (LocalizationHelper.IsRTL())
            {
                if (RtlPositionTr == null)
                {
                    Debug.LogError(nameof(RtlPositionTr) + " is null");
                }
                else
                {
                    success = true;
                    Instantiate(_marker, RtlPositionTr.position, Quaternion.identity, this.transform);
                }
            }
            else
            {
                if (LtrPositionTr == null)
                {
                    Debug.LogError(nameof(LtrPositionTr) + " is null");
                }
                else
                {
                    success = true;
                    Instantiate(_marker, LtrPositionTr.position, Quaternion.identity, this.transform);
                }
            }
            if (!success)//fallback
                Instantiate(_marker, this.transform.position, Quaternion.identity, this.transform);
            // ------
            isLevelReached = true;
        }
        else
        {
            isLevelReached = false;
            _myImg.sprite = _lockedSprite;
            _levelIndexGameObject.SetActive(isLevelReached);
            for (int i = 0; i < _unfilledStars.Length; i++)
            {
                _unfilledStars[i].SetActive(false);
            }
        }
        _lockIcon.SetActive(!isLevelReached);
    }
}