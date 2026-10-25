using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemMover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Item item;
    private Canvas canvas;
    private LayoutElement layoutElement;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isFloating = true;
    private float floatTimer = 0f;
    private float floatSpeed;
    private float floatRadius;
    private Vector2 floatCenterPosition;
    private bool isInBox = false;
    private float randomOffset;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }

        item = GetComponent<Item>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
        floatCenterPosition = rectTransform.anchoredPosition;

        floatSpeed = Random.Range(0.8f, 2.5f);
        floatRadius = Random.Range(30f, 70f);
        randomOffset = Random.Range(0f, 6.28f);
        floatTimer = randomOffset;

        isFloating = true;
    }

    void Update()
    {
        if (isFloating && !isDragging)
        {
            floatTimer += Time.deltaTime * floatSpeed;

            float floatX = Mathf.Cos(floatTimer) * floatRadius;
            float floatY = Mathf.Sin(floatTimer) * floatRadius;

            rectTransform.anchoredPosition = floatCenterPosition + new Vector2(floatX, floatY);
        }
    }

    private bool isDragging = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
        transform.localScale = originalScale;

        
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("BubblePop");
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (eventData.pointerDrag == null)
        {
            ResetPosition();
        }
    }

    public void ResetPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        floatCenterPosition = originalPosition;
        transform.localScale = originalScale;
        isFloating = true;
        isInBox = false;
        floatTimer = randomOffset;
    }

    public void PlaceInBox(float scaleFactor, Vector2 boxPosition)
    {
        transform.localScale = originalScale * scaleFactor;
        floatCenterPosition = boxPosition;
        isInBox = true;
    }

    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }

    public bool IsInBox()
    {
        return isInBox;
    }
}