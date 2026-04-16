using UnityEngine;

[CreateAssetMenu(fileName = "NewKidItem", menuName = "KidGame/Item Data")]
public class KidItemData : ScriptableObject
{
    public string itemTag;      
    public Sprite itemSprite;    
}