using UnityEngine;
using UnityEngine.UI;

public class RoboPathDropSlot : MonoBehaviour
{
    public RoboPathDragImage PlacedCard { get; private set; }

    public RoboPathCore.RobotAction PlacedAction =>
        PlacedCard != null ? PlacedCard.actionType : RoboPathCore.RobotAction.None;

    public bool IsFilled => PlacedCard != null;

    private Color originalColor;

    
    public void PlaceCard(RoboPathDragImage card)
    {
        
        if (PlacedCard != null)
            Destroy(PlacedCard.gameObject);

        PlacedCard = card;

        RectTransform cardRect = card.GetComponent<RectTransform>();

        
        cardRect.SetParent(transform, false);
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;

        
        Image img = card.GetComponent<Image>();
        if (img != null)
            originalColor = img.color;
    }

    
    public void LiftCard()
    {
        PlacedCard = null;
    }

    
    public void RemoveCard()
    {
        if (PlacedCard != null)
        {
            Destroy(PlacedCard.gameObject);
            PlacedCard = null;
        }
    }

    
    public void SetCardColor(Color c)
    {
        if (PlacedCard != null)
            PlacedCard.SetColor(c);
    }

    public void ResetCardColor()
    {
        if (PlacedCard != null)
            PlacedCard.SetColor(originalColor);
    }
}