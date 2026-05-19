using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Over_2_SpaceShipOptionPanel : MonoBehaviour
{
    private Game_Over_2_AudioManager _audioManager;



    [SerializeField] private Toggles _sfxTaggles;
    [SerializeField] private Toggles _musicTaggle;
    [SerializeField] private Toggles _gyroTaggle;
    public static bool sfxMuted = false, musicMuted = false;
    [HideInInspector] public bool gyroOff = false;

    [SerializeField] private GameObject _SettingsPanel;

    [SerializeField] private Animator _anim;

    [SerializeField] private GameObject _canvas;




    private void Start()
    {
        _audioManager = Game_Over_2_AudioManager.audioManInstance;
        

        Switchs_State();
    }

    private void Switchs_State()
    {
        sfxMuted = (PlayerPrefs.GetInt(Game_Over_2_Constants.SFX_STATE, 1) != 1);
        musicMuted = (PlayerPrefs.GetInt(Game_Over_2_Constants.MUSIC_STATE, 1) != 1);
        gyroOff = (PlayerPrefs.GetInt(Game_Over_2_Constants.GYRO_STATE, 1) != 1);

        if (sfxMuted)
        {
            _sfxTaggles.onSwitch.SetActive(false);
            _sfxTaggles.offSwitch.SetActive(true);
        }
        else
        {
            _sfxTaggles.onSwitch.SetActive(true);
            _sfxTaggles.offSwitch.SetActive(false);
        }

        if (musicMuted)
        {
            _musicTaggle.onSwitch.SetActive(false);
            _musicTaggle.offSwitch.SetActive(true);
        }
        else
        {
            _musicTaggle.onSwitch.SetActive(true);
            _musicTaggle.offSwitch.SetActive(false);
        }
        if (gyroOff)
        {
            _gyroTaggle.onSwitch.SetActive(false);
            _gyroTaggle.offSwitch.SetActive(true);
        }
        else
        {
            _gyroTaggle.onSwitch.SetActive(true);
            _gyroTaggle.offSwitch.SetActive(false);
        }
    }

    public void Open_Close_Settings_Panel(bool isOpened)
    {
        _canvas.SetActive(true);
        _SettingsPanel.SetActive(isOpened);

    }

    public void Mute_Sfx()
    {
        if (sfxMuted)
        {
            sfxMuted = false;
            PlayerPrefs.SetInt(Game_Over_2_Constants.SFX_STATE, 1);
        }
        else
        {
            sfxMuted = true;
            PlayerPrefs.SetInt(Game_Over_2_Constants.SFX_STATE, 0);
        }
        _sfxTaggles.onSwitch.SetActive(!sfxMuted);
        _sfxTaggles.offSwitch.SetActive(sfxMuted);
        _audioManager.Mute_Sfx(sfxMuted);

    }

    public void Mute_Music()
    {
        if (musicMuted)
        {
            musicMuted = false;
            PlayerPrefs.SetInt(Game_Over_2_Constants.MUSIC_STATE, 1);
        }
        else
        {
            musicMuted = true;
            PlayerPrefs.SetInt(Game_Over_2_Constants.MUSIC_STATE, 0);
        }
        _musicTaggle.onSwitch.SetActive(!musicMuted);
        _musicTaggle.offSwitch.SetActive(musicMuted);
        _audioManager.Mute_Music(musicMuted);
    }

    public void Gyro_Off()
    {
        if (gyroOff)
        {
            gyroOff = false;
            PlayerPrefs.SetInt(Game_Over_2_Constants.GYRO_STATE, 1);
        }
        else
        {
            gyroOff = true;
            PlayerPrefs.SetInt(Game_Over_2_Constants.GYRO_STATE, 0);
        }
        _gyroTaggle.onSwitch.SetActive(!gyroOff);
        _gyroTaggle.offSwitch.SetActive(gyroOff);
    }
}