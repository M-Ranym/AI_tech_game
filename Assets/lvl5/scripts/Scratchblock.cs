using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScratchBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BlockType blockType;
    public bool isInToolbox = true;

    [Header("Puzzle Sprites")]
    public Sprite spriteTop;
    public Sprite spriteMiddle;
    public Sprite spriteBottom;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private GameObject dragClone = null;

    private bool inPanel = false;
    private int lastInsertIndex = -1;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetPuzzleShape(int index, int total)
    {
        Image img = GetComponent<Image>();
        if (img == null) return;

        Sprite next = (index == 0) ? spriteTop
                    : (index == total - 1) ? spriteBottom
                    : spriteMiddle;

        if (next != null) img.sprite = next;
    }

    // 🎤 NEW — lets an external input method (voice commands) place a
    // block directly into the program panel, bypassing drag-and-drop.
    // The block behaves identically to a dragged one afterwards (it can
    // still be reordered or removed by dragging it).
    public void InitializeAsPlacedBlock(BlockType type, ProgramPanel panel)
    {
        blockType = type;
        isInToolbox = false;
        inPanel = true;
        panel.AddBlock(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastInsertIndex = -1;

        if (isInToolbox)
        {
            dragClone = Instantiate(gameObject, canvas.transform, true);
            dragClone.transform.position = eventData.position;

            var cs = dragClone.GetComponent<ScratchBlock>();
            cs.isInToolbox = false;
            cs.inPanel = false;
            cs.spriteTop = spriteTop;
            cs.spriteMiddle = spriteMiddle;
            cs.spriteBottom = spriteBottom;

            dragClone.GetComponent<CanvasGroup>().blocksRaycasts = false;

            // Drag sound
            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("BlockDrag");
        }
        else if (inPanel)
        {
            ProgramPanel panel = FindObjectOfType<ProgramPanel>();
            if (panel != null)
                panel.SetDragging(gameObject);

            inPanel = false;
            transform.SetParent(canvas.transform, false);
            transform.SetAsLastSibling();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.9f;

            // Drag sound
            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("BlockDrag");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isInToolbox)
        {
            if (dragClone != null)
                dragClone.transform.position = eventData.position;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            rectTransform.anchoredPosition = localPoint;

            ProgramPanel panel = FindObjectOfType<ProgramPanel>();

            if (panel != null && IsOverPanel(panel, eventData))
            {
                int idx = panel.GetInsertIndex(eventData.position.y);

                lastInsertIndex = idx;
                panel.SetInsertIndex(idx);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ProgramPanel panel = FindObjectOfType<ProgramPanel>();
        bool onPanel = panel != null && IsOverPanel(panel, eventData);

        if (isInToolbox)
        {
            if (dragClone != null)
            {
                if (onPanel)
                {
                    var cloneSB = dragClone.GetComponent<ScratchBlock>();
                    cloneSB.inPanel = true;
                    dragClone.GetComponent<CanvasGroup>().blocksRaycasts = true;
                    dragClone.transform.SetParent(panel.transform, true);
                    panel.AddBlock(dragClone);

                    // Dropped on panel sound
                    if (Game_Over_2_AudioManager.audioManInstance != null)
                        Game_Over_2_AudioManager.audioManInstance.Play_Sfx("BlockDropPanel");
                }
                else
                {
                    // Dropped outside sound
                    if (Game_Over_2_AudioManager.audioManInstance != null)
                        Game_Over_2_AudioManager.audioManInstance.Play_Sfx("BlockDropOut");
                    Destroy(dragClone);
                }

                dragClone = null;
            }
        }
        else
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (onPanel)
            {
                transform.SetParent(panel.transform, true);
                panel.FinalizeReorder(gameObject);
                inPanel = true;
                panel.SnapToTargets();

                // Reordered in panel sound
                if (Game_Over_2_AudioManager.audioManInstance != null)
                    Game_Over_2_AudioManager.audioManInstance.Play_Sfx("BlockDropPanel");
            }
            else
            {
                panel?.RemoveBlock(gameObject);

                // Removed from panel sound
                if (Game_Over_2_AudioManager.audioManInstance != null)
                    Game_Over_2_AudioManager.audioManInstance.Play_Sfx("BlockDropOut");

                Destroy(gameObject);
            }

            panel?.ClearDragging();
        }

        lastInsertIndex = -1;
    }

    bool IsOverPanel(ProgramPanel panel, PointerEventData eventData)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            panel.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera);
    }
}