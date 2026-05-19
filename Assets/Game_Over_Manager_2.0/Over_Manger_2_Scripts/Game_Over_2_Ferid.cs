using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class Game_Over_2_Ferid : MonoBehaviour
{
    // public UnityEngine.U2D.Animation.SpriteResolver upperAccessory;
    // public UnityEngine.U2D.Animation.SpriteResolver downAccessory;
    public Animator myAnim;

    public AudioSource myAudioSource;

    public static bool isSkinChanged = false;

    private bool isTalking = false;

    private float[] clipSampleData = new float[1024];

    private float clipLoudness;
    private float currentUpdateTime;

    private const float EPSILON = 0.001f;

    private void Awake()
    {
        ChangeMySkin(false);
    }

    private void Start()
    {
        clipSampleData = new float[1024];
    }

    private void ChangeMySkin(bool canPlayAnim)
    {
        //upperAccessory.SetCategoryAndLabel(Constants.UPPER_SPRITE_RESOLVER_CATEGORY, Game_Over_2_Manager.instance.Get_User().avatarUpperAccessories.ToString());
        //downAccessory.SetCategoryAndLabel(Constants.DOWN_SPRITE_RESOLVER_CATEGORY, Game_Over_2_Manager.instance.Get_User().avatarLowerAccessories.ToString());

        //if (canPlayAnim) myAnim.Play(Constants.ANIM_FERID_NEW_CLOTHES, -1, 0);
    }

    private void Update()
    {
        if (isSkinChanged)
        {
            ChangeMySkin(true);
            isSkinChanged = false;
        }

        if (isTalking)
        {
            Analyze_Speech_Data();
        }
    }

    public void Talk(AudioClip speech)
    {
        isTalking = true;
        myAudioSource.clip = speech;
        currentUpdateTime = 0;
        myAudioSource.Stop();
        myAudioSource.clip = speech;
        myAudioSource.Play();
    }

    private void Analyze_Speech_Data()
    {
        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= 0.1f)
        {
            currentUpdateTime = 0f;
            try
            {
                if (clipSampleData == null || myAudioSource == null)
                {
                    Debug.LogError(nameof(clipSampleData) + " or " + nameof(myAudioSource) + " is null");
                }
                else
                {
                    myAudioSource.clip.GetData(clipSampleData, myAudioSource.timeSamples); //I read 512 samples
                    clipLoudness = 0f;
                    foreach (var sample in clipSampleData)
                    {
                        clipLoudness += Mathf.Abs(sample);
                    }
                    clipLoudness /= 1024; //clipLoudness is what you are looking for

                    if (clipLoudness > EPSILON)
                    {
                        myAnim.SetLayerWeight(1, 1);
                    }
                    else
                    {
                        myAnim.SetLayerWeight(1, 0);
                    }
                }
                   
            }
            catch (Exception e)
            {
                Debug.Log("Sound Samples failed " + e.Message);
            }
        }
    }
}