using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RoboPathDragImage : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Action this card represents")]
    public RoboPathCore.RobotAction actionType;

    [Header("The panel that contains all slots")]
    public RectTransform answerPanel;

    private GameObject dragObject;
    private RectTransform dragRect;
    private RectTransform myRect;
    private Canvas rootCanvas;
    private Image myImage;
    public bool isPlacedInSlot = false;

    void Awake()
    {
        myRect = GetComponent<RectTransform>();
        myImage = GetComponent<Image>();
        rootCanvas = GetComponentInParent<Canvas>();
        while (rootCanvas != null && !rootCanvas.isRootCanvas)
            rootCanvas = rootCanvas.transform.parent?.GetComponentInParent<Canvas>();
    }

    public void MarkAsPlaced() => isPlacedInSlot = true;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rootCanvas == null) return;

        if (isPlacedInSlot)
        {
            isPlacedInSlot = false;
            RoboPathSlotManager.Instance.HandleLift(this);

            transform.SetParent(rootCanvas.transform, true);
            transform.SetAsLastSibling();

            myImage.color = new Color(myImage.color.r, myImage.color.g, myImage.color.b, 0.85f);

            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            dragObject = gameObject;
            dragRect = myRect;
        }
        else
        {
            dragObject = Instantiate(gameObject, rootCanvas.transform);
            dragObject.name = "DragClone";
            dragObject.transform.SetAsLastSibling();

            Destroy(dragObject.GetComponent<RoboPathDragImage>());

            dragRect = dragObject.GetComponent<RectTransform>();
            dragRect.sizeDelta = myRect.sizeDelta;

            Image cloneImg = dragObject.GetComponent<Image>();
            cloneImg.sprite = myImage.sprite;
            cloneImg.color = new Color(myImage.color.r, myImage.color.g, myImage.color.b, 0.85f);

            CanvasGroup cg = dragObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = dragObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RoboCardDrag");

        MoveToPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
            MoveToPointer(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject == null) return;

        if (!IsPointerInsidePanel(eventData.position))
        {
            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RoboCardDropOut");
            Destroy(dragObject);
            dragObject = null;
            return;
        }

        Image img = dragObject.GetComponent<Image>();
        if (img != null) img.color = myImage.color;

        CanvasGroup cg = dragObject.GetComponent<CanvasGroup>();
        if (cg != null) Destroy(cg);

        RoboPathDragImage cardScript = dragObject.GetComponent<RoboPathDragImage>();
        if (cardScript == null) cardScript = dragObject.AddComponent<RoboPathDragImage>();
        cardScript.actionType = actionType;
        cardScript.answerPanel = answerPanel;
        cardScript.isPlacedInSlot = false;

        RoboPathSlotManager.Instance.HandleDrop(cardScript, eventData.position);

        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RoboCardDropSlot");

        dragObject = null;
    }

    public void SetColor(Color c) => myImage.color = c;

    private bool IsPointerInsidePanel(Vector2 screenPos)
    {
        if (answerPanel == null) return true;
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : rootCanvas.worldCamera;
        return RectTransformUtility.RectangleContainsScreenPoint(answerPanel, screenPos, cam);
    }

    private void MoveToPointer(PointerEventData eventData)
    {
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : rootCanvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            eventData.position, cam, out Vector2 local);
        dragRect.anchoredPosition = local;
    }
}