using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class Game_Over_2_Ferid : MonoBehaviour
{
    
    
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
                    myAudioSource.clip.GetData(clipSampleData, myAudioSource.timeSamples); 
                    clipLoudness = 0f;
                    foreach (var sample in clipSampleData)
                    {
                        clipLoudness += Mathf.Abs(sample);
                    }
                    clipLoudness /= 1024; 

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