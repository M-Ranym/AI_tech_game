using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ferid_Speach
{
    public List<AudioClip> listOfFact = new List<AudioClip>();
    public List<string> listOfinformation = new List<string>();
}

public class Game_Over_2_Ferid_Talks : MonoBehaviour
{
    [SerializeField]
    private int index = 11;

    [SerializeField] private Animator _anim;

    private Game_Over_2_AudioManager _audioManager;

    [SerializeField] private float _timeOfFeridToTalk = 10f;
    public float _currentTime = 0;

    [SerializeField] private Ferid_Speach _feridIdleTalksLists;
    private int _indexOfTalkType = 1;

    private static bool _isSayingHello = true;

    private Game_Over_2_Ferid ferid;

    // Start is called before the first frame update
    private void Start()
    {
        _audioManager = Game_Over_2_AudioManager.audioManInstance;
        ferid = GetComponent<Game_Over_2_Ferid>();
        if (_isSayingHello)
        {
            _isSayingHello = false;

            StartCoroutine(Say_Hi());
        }
    }

    private IEnumerator Say_Hi()
    {
        yield return new WaitForSeconds(1.5f);
        int ran = Random.Range(0, index);
        _audioManager.Ferid_Talking(Game_Over_2_Constants.FERID_TALKS_HELLO + ran.ToString());
    }

    private void Update()
    {
        //_currentTime += Time.deltaTime;
        //if (_currentTime > _timeOfFeridToTalk)
        //{
        //    int ran = Random.Range(0, _feridIdleTalksLists.listOfinformation.Count);
        //    _audioManager.Ferid_Talking(_feridIdleTalksLists.listOfinformation[ran]);
        //    _currentTime = 0;
        //}
    }
}