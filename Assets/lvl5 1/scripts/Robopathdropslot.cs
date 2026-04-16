using UnityEngine;
using UnityEngine.UI;

public class RoboPathDropSlot : MonoBehaviour
{
    public RoboPathDragImage PlacedCard { get; private set; }

    public RoboPathCore.RobotAction PlacedAction =>
        PlacedCard != null ? PlacedCard.actionType : RoboPathCore.RobotAction.None;

    public bool IsFilled => PlacedCard != null;

    private Color originalColor;

    // ── Place ───────────────────────────────────
    public void PlaceCard(RoboPathDragImage card)
    {
        // Remove old card if exists
        if (PlacedCard != null)
            Destroy(PlacedCard.gameObject);

        PlacedCard = card;

        RectTransform cardRect = card.GetComponent<RectTransform>();

        // Snap into slot
        cardRect.SetParent(transform, false);
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;

        // Store original color safely
        Image img = card.GetComponent<Image>();
        if (img != null)
            originalColor = img.color;
    }

    // ── Lift (drag again) ───────────────────────
    public void LiftCard()
    {
        PlacedCard = null;
    }

    // ── Remove completely ───────────────────────
    public void RemoveCard()
    {
        if (PlacedCard != null)
        {
            Destroy(PlacedCard.gameObject);
            PlacedCard = null;
        }
    }

    // ── Visual feedback ─────────────────────────
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