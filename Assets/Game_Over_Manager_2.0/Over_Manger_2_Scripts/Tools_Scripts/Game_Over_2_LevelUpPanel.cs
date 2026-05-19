using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UPersian.Components;

public class Game_Over_2_LevelUpPanel : MonoBehaviour
{
    [SerializeField] private RtlText _levelUpTxt;
    private Game_Over_2_Manager _gameOverMan;

    [SerializeField] private Game_Over_2_LoadScene _loadScene;

    [SerializeField] private AudioClip _levelUpsfx;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _gameOverMan = Game_Over_2_Manager.instance;

        _audioSource.PlayOneShot(_levelUpsfx);
        _levelUpTxt.text = PlayerPrefs.GetInt(Game_Over_2_Constants.GAME_DATA_PLAYER_LEVEL).ToString();
    }

    public void Load_Map_Scene()
    {
        string path = Game_Over_2_Constants.SCENE_PREFIX + Game_Over_2_Constants.MAP;
        _loadScene.LoadScene(path);
    }
}