using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TargetBox : MonoBehaviour, IDropHandler
{
    public string acceptedType;
    private int itemCount = 0;

    public float boxWidth = 200f;
    public float boxHeight = 250f;
    public float itemScaleFactor = 0.7f;

    private float feedbackDuration = 0.8f;
    private float feedbackTimer = 0f;

    private Image boxImage;
    private Color originalColor;

    // cached reference – found once in Start
    private TutorialManager _tutorial;

    void Start()
    {
        boxImage = GetComponent<Image>();
        if (boxImage != null)
            originalColor = boxImage.color;

        _tutorial = FindObjectOfType<TutorialManager>();
    }

    void Update()
    {
        if (feedbackTimer > 0)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0)
                if (boxImage != null) boxImage.color = originalColor;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        ItemMover mover = eventData.pointerDrag.GetComponent<ItemMover>();
        Item item = eventData.pointerDrag.GetComponent<Item>();

        if (mover == null || item == null) return;

        bool correct = item.itemType == acceptedType;

        // ── Notify TutorialManager about the drop (regardless of correct/wrong) ──
        if (_tutorial != null && _tutorial.IsTutorialActive())
            _tutorial.OnTutorialDrop(eventData.pointerDrag, correct);

        RectTransform itemRect = mover.GetComponent<RectTransform>();
        Vector2 positionInBox = Vector2.zero;

        if (correct)
        {
            mover.transform.localScale = mover.GetOriginalScale() * itemScaleFactor;
            mover.transform.SetParent(transform, false);
            itemRect.anchoredPosition = positionInBox;
            mover.PlaceInBox(itemScaleFactor, positionInBox);
            itemCount++;

            if (boxImage != null) boxImage.color = Color.green;
            feedbackTimer = feedbackDuration;

            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("DropCorrect");

            MyGameManager.instance.ItemPlaced();
        }
        else
        {
            if (boxImage != null) boxImage.color = Color.red;
            feedbackTimer = feedbackDuration;

            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("DropWrong");

            mover.ResetPosition();
        }
    }

    public int GetItemCount() => itemCount;
    public void ClearBox() => itemCount = 0;
}