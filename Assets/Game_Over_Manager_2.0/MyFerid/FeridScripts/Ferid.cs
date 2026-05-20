using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ferid : MonoBehaviour
{

    public UnityEngine.U2D.Animation.SpriteResolver upperAccessory;
    public UnityEngine.U2D.Animation.SpriteResolver downAccessory;
    public Animator myAnim;

    public static bool isSkinChanged = false;

    bool isTalking = false;

    void Awake()
    {
        ChangeMySkin();
    }

    void ChangeMySkin()
    {
        upperAccessory.SetCategoryAndLabel("Upper_Accessory", "0");
        downAccessory.SetCategoryAndLabel("Down_Accessory", "0");

        myAnim.Play("NewClothes");
    }

    public void Talk()
    {
        isTalking = true;
    }

    public void Shut_Up()
    {
        isTalking = false;
    }

    void Update()
    {
        if (isSkinChanged)
        {
            ChangeMySkin();
            isSkinChanged = false;
        }
        myAnim.SetBool("isTalking", isTalking);
    }

}
