using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using JetBrains.Annotations;
using NoSuchStudio.Localization;
using System.Collections.Generic;
using com.horizon.LocalizationSystem;

public class Game_Over_2_AudioManager : MonoBehaviour
{
    [SerializeField] private Game_Over_2_Sound[] _sounds;
    [SerializeField] private Game_Over_2_Sound[] _feridSounds;

    [SerializeField] private AudioSource externalAudioSource;

    public static Game_Over_2_AudioManager audioManInstance;
    string sceneName = "";

    private Game_Over_2_Ferid _ferid;
    public float musicMoment = 0;

    private void Awake()
    {
        if (audioManInstance == null)
        {
            audioManInstance = this;
            sceneName = SceneManager.GetActiveScene().name;
        }
        else if (audioManInstance.sceneName != SceneManager.GetActiveScene().name)
        {
            if (Get_Music_Clip() == audioManInstance.Get_Music_Clip())
                musicMoment = audioManInstance.musicMoment;
            else
                musicMoment = 0;

            Destroy(audioManInstance.gameObject);

            audioManInstance = this;
            sceneName = SceneManager.GetActiveScene().name;

            //return;
        }
        else
        {
            return;
        }

        DontDestroyOnLoad(gameObject);
        Init();
    }

    void Init()
    {
        foreach (Game_Over_2_Sound s in _sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;//use s.clip instead of GetLocalizedAudioClip cuz its a non-text audio
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        //---This section is useless (remove after re-verifying)
        //foreach (Game_Over_2_Sound s in _feridSounds)
        //{
        //    if (s.source != null)
        //        Destroy(s.source);

        //    s.source = gameObject.AddComponent<AudioSource>();
        //    //s.source.clip = s.clip;
        //    s.source.clip = s.GetLocalizedAudioClip();//use this fn cuz it s a text audio , thus it requires localization
        //    s.source.volume = s.volume;
        //    s.source.pitch = s.pitch;
        //    s.source.loop = s.loop;
        //}
        //--------



        if (PlayerPrefs.GetInt(Constants.SOUND_ENABLED, 1) == 1)
            Play_Music("Music");
    }

    public AudioClip Get_Music_Clip()
    {
        foreach (Game_Over_2_Sound s in _sounds)
        {
            if (s.name == "Music")
            {
                if (s.source != null)
                {
                    musicMoment = s.source.time;
                }

                return s.clip;//non text audio
            }
        }
        Debug.Log("No Music Found");

        return null;
    }

    public void Ferid_Talking(string name)
    {
        _ferid = FindObjectOfType<Game_Over_2_Ferid>();
        if (!Game_Over_2_OptionPanel.sfxMuted)
        {
            Game_Over_2_Sound s = Array.Find(_feridSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound " + name + " not Found!");
            }

            if (_ferid != null)
                _ferid.Talk(s.GetLocalizedAudioClip());//use this fn cuz it s a text audio , thus it requires localization
        }
    }

    public void PlayExternalSound(AudioClip myAudioClip)
    {
        externalAudioSource.PlayOneShot(myAudioClip);
        StartCoroutine(MusicFadeLogic());
    }

    IEnumerator MusicFadeLogic()
    {
        if (PlayerPrefs.GetInt(Game_Over_2_Constants.MUSIC_STATE, 1) == 1)
        {
            Mute_Music(true);
        }

        while (externalAudioSource.isPlaying)
        {
            yield return null;
        }

        if (PlayerPrefs.GetInt(Game_Over_2_Constants.MUSIC_STATE, 1) == 1)
        {
            Mute_Music(false);
        }
    }

    public void Play_Sfx(string name)
    {
        Game_Over_2_Sound s = Array.Find(_sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not Found!");
        }
        s.source.Play();
    }

    public void Stop_Sfx(string name)
    {
        Game_Over_2_Sound s = Array.Find(_sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not Found!");
        }
        s.source.Stop();
    }

    public void Mute_Sfx(bool m)
    {
        foreach (Game_Over_2_Sound s in _sounds)
        {
            if (s.name != "Music")
            {
                s.source.mute = m;
            }
        }
        foreach (Game_Over_2_Sound sound in _feridSounds)
        {
            sound.source.mute = m;
        }
    }

    public void Play_Music(string name)
    {
        Game_Over_2_Sound s = Array.Find(_sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not Found!");
        }

        if (musicMoment > 0)
            s.source.time = musicMoment;

        s.source.pitch = 1.05f;
        s.source.Play();
    }

    public void Play_Music(AudioClip music)
    {        
        Game_Over_2_Sound s = Array.Find(_sounds, sound => sound.name == "Music");
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not Found!");
        }

        if (musicMoment > 0)
            s.source.time = musicMoment;

        s.source.clip = music;
        s.source.pitch = 1.05f;
        s.source.Play();
    }

    public void Mute_Music(bool m)
    {
        foreach (Game_Over_2_Sound s in _sounds)
        {
            if (s.name == "Music")
            {
                s.source.mute = m;
                break;
            }
        }
    }
    //make sure this is used for sfx , not text based audioclips
    public void PlayOneShot_Sfx(string name)
    {
        Game_Over_2_Sound s = Array.Find(_sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not Found!");
        }
        s.source.PlayOneShot(s.clip);
    }
}

[Serializable]
public class Game_Over_2_Sound
{
    //--- localization
    public bool IsFeridSpeech = false;//to distinguish between sfx and speeches(audio that have text read)
    //--
    public string name;

    public AudioClip clip; // fallback in case of speech-audio , but it is required in case of sfx audio (non speech)

    //--- localization
    [Tooltip("Ignore this Field for non-text audio , use the default clip")]
    public List<LocalizedAudioClip> LocalizedAudioClipsList;
    //---


    [Range(0f, 1f)]
    public float volume;

    [Range(0.1f, 1f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

    //---localization
    public AudioClip GetLocalizedAudioClip()
    {
        //in case of sfx (no speech) just return the default clip
        if (!IsFeridSpeech)
        {
            if (clip == null) 
                Debug.LogError(nameof(clip) + $" ref of sfx ({name}) is null"); 
            return clip;
        }

        //in case of speech
        if (LocalizedAudioClipsList == null || LocalizedAudioClipsList.Count == 0) { 
            Debug.LogError(nameof(LocalizedAudioClipsList) + $" is null or empty  [audioName = {name}], thus the default will be returned"); 
            return clip; 
        }

        Locale? appLang = LocalizationHelper.GetCurrentLanugage();
        if (appLang == null) 
        {
            Debug.LogError(nameof(appLang) + $" is null [audioName = {name}], thus the default will be returned ");
            return clip; 
        }
        
        
        for (int i = 0; i < LocalizedAudioClipsList.Count; i++)
        {
            LocalizedAudioClip current = LocalizedAudioClipsList[i];
            if(current == null)
            {
                Debug.LogError(nameof(LocalizedAudioClipsList) + "["+i+"] is null");
                continue;
            }
            if (!current.IsValid())
                continue;

            if(current.Language == appLang)
            {
                return current.AudioClip;
            }
        }

        Debug.LogError($"Audioclip for language ${appLang} is Not found, thus the default will be returned");
        return clip;
    }
    //-----
}
//---Localization
[Serializable]
public class LocalizedAudioClip
{
    public Locale Language;
    public AudioClip AudioClip;


    public bool IsValid()
    {
        if (AudioClip == null) { Debug.LogError(nameof(AudioClip) + " is null"); return false; }

        return true;
    }
}


//--

[Serializable]
public struct Toggles
{
    public GameObject onSwitch;
    public GameObject offSwitch;
}