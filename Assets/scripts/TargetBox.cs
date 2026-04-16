using UnityEngine;
using UnityEngine.EventSystems;

public class TargetBox : MonoBehaviour, IDropHandler
{
    public string acceptedType;
    private int itemCount = 0;

    public float boxWidth = 200f;
    public float boxHeight = 250f;
    public float itemScaleFactor = 0.7f;

    // Feedback images for right and wrong
    public GameObject correctFeedback; // Green box (drag here in inspector)
    public GameObject wrongFeedback;   // Red box (drag here in inspector)

    private float feedbackDuration = 0.8f;
    private float feedbackTimer = 0f;

    private RectTransform boxRect;
    private Canvas canvas;

    void Start()
    {
        boxRect = GetComponent<RectTransform>();
        if (boxRect == null)
        {
            boxRect = gameObject.AddComponent<RectTransform>();
        }
        canvas = GetComponentInParent<Canvas>();

        if (correctFeedback != null)
        {
            correctFeedback.SetActive(false);
            RectTransform correctRect = correctFeedback.GetComponent<RectTransform>();
            if (correctRect != null)
            {
                correctRect.anchoredPosition = boxRect.anchoredPosition;
            }
        }
        if (wrongFeedback != null)
        {
            wrongFeedback.SetActive(false);
            RectTransform wrongRect = wrongFeedback.GetComponent<RectTransform>();
            if (wrongRect != null)
            {
                wrongRect.anchoredPosition = boxRect.anchoredPosition;
            }
        }
    }

    void Update()
    {
        if (feedbackTimer > 0)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0)
            {
                HideFeedback();
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            ItemMover mover = eventData.pointerDrag.GetComponent<ItemMover>();
            Item item = eventData.pointerDrag.GetComponent<Item>();

            if (mover != null && item != null)
            {
                RectTransform itemRect = mover.GetComponent<RectTransform>();
                Vector2 positionInBox = Vector2.zero;

                if (item.itemType == acceptedType)
                {
                    // Scale the item
                    mover.transform.localScale = mover.GetOriginalScale() * itemScaleFactor;

                    // Keep animal in normal box
                    mover.transform.SetParent(transform, false);
                    itemRect.anchoredPosition = positionInBox;

                    mover.PlaceInBox(itemScaleFactor, positionInBox);
                    itemCount++;

                    ShowCorrectFeedback();
                    GameManager.instance.ItemPlaced();
                }
                else
                {
                    ShowWrongFeedback();
                    mover.ResetPosition();
                }
            }
        }
    }

    private void ShowCorrectFeedback()
    {
        if (correctFeedback != null)
        {
            // Clear old copies
            foreach (Transform child in correctFeedback.transform)
            {
                Destroy(child.gameObject);
            }

            // Copy ALL animals from normal box to green box
            foreach (Transform child in transform)
            {
                ItemMover originalMover = child.GetComponent<ItemMover>();
                if (originalMover != null)
                {
                    // Create copy with full functionality
                    GameObject animalCopy = Instantiate(child.gameObject, correctFeedback.transform);
                    animalCopy.transform.localScale = child.localScale;

                    RectTransform copyRect = animalCopy.GetComponent<RectTransform>();
                    if (copyRect != null)
                    {
                        copyRect.anchoredPosition = child.GetComponent<RectTransform>().anchoredPosition;
                    }

                    // Keep the ItemMover script active so it floats
                    ItemMover copyMover = animalCopy.GetComponent<ItemMover>();
                    if (copyMover != null)
                    {
                        // Don't disable it - keep it active so floating continues
                    }

                    CanvasGroup copyCanvasGroup = animalCopy.GetComponent<CanvasGroup>();
                    if (copyCanvasGroup != null)
                    {
                        copyCanvasGroup.blocksRaycasts = false;
                    }
                }
            }

            correctFeedback.SetActive(true);
            feedbackTimer = feedbackDuration;
        }
    }

    private void ShowWrongFeedback()
    {
        if (wrongFeedback != null)
        {
            wrongFeedback.SetActive(true);
            feedbackTimer = feedbackDuration;
        }
    }

    private void HideFeedback()
    {
        if (correctFeedback != null)
        {
            correctFeedback.SetActive(false);
            // Clear copies from green box
            foreach (Transform child in correctFeedback.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (wrongFeedback != null)
            wrongFeedback.SetActive(false);
    }

    public int GetItemCount()
    {
        return itemCount;
    }

    public void ClearBox()
    {
        itemCount = 0;
    }
}