using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;


public class Game_Over_2_User_Avatar_Image_Frame : MonoBehaviour
{
    public Image avatarBGImg;
  //  public UnityEngine.U2D.Animation.SpriteResolver headSpriteResolver;

    [SerializeField] private SpriteRenderer _headRenderer;
    [SerializeField] private Image _headImage;

    public void Set_Avatar_Parameters(string headAceesorySpriteName) //Color avatarBGColor)
    {
        Set_Accessory_Img(headAceesorySpriteName);
        _headImage.sprite = _headRenderer.sprite;
        Set_Avatar_BG_Color();
    }

    private void Set_Accessory_Img(string headAceesorySpriteName)
    {
        //try
        //{
        //    this.headSpriteResolver.SetCategoryAndLabel(Constants.UPPER_SPRITE_RESOLVER_CATEGORY, headAceesorySpriteName);
        //}catch(System.Exception e)
        //{
        //    Debug.Log(e.Message);
        //}
    }

    private void Set_Avatar_BG_Color()
    {
        //Color col = Random.ColorHSV();
        //this.avatarBGImg.color = col;
    }
}