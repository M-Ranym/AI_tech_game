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
    private GameObject dragClone;
    private bool inPanel = false;

    // Used during live reorder so we don't re-add ourselves every frame
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

    // ── Begin drag ────────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        lastInsertIndex = -1;

        if (isInToolbox)
        {
            // Spawn a clone that follows the cursor; original stays in toolbox
            dragClone = Instantiate(gameObject, canvas.transform, true);
            var cs = dragClone.GetComponent<ScratchBlock>();
            cs.isInToolbox = false;
            cs.inPanel = false;
            cs.spriteTop = spriteTop;
            cs.spriteMiddle = spriteMiddle;
            cs.spriteBottom = spriteBottom;
            dragClone.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else if (inPanel)
        {
            // Lift out of panel — panel re-layouts without us, others close the gap
            ProgramPanel panel = FindObjectOfType<ProgramPanel>();
            if (panel != null) panel.RemoveBlock(gameObject);
            inPanel = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // ── During drag ───────────────────────────────────────────────────────────
    public void OnDrag(PointerEventData eventData)
    {
        if (isInToolbox && dragClone != null)
        {
            dragClone.transform.position = eventData.position;
        }
        else
        {
            // Move self with cursor
            transform.position = eventData.position;

            // Live reorder preview: if cursor is over the panel, show where it
            // would land by temporarily inserting and repositioning
            ProgramPanel panel = FindObjectOfType<ProgramPanel>();
            if (panel != null && IsOverPanel(panel, eventData))
            {
                int idx = panel.GetInsertIndex(eventData.position.y);
                if (idx != lastInsertIndex)
                {
                    lastInsertIndex = idx;
                    // Temporarily insert to show preview positions of other blocks
                    panel.ReorderBlock(gameObject, idx);
                    // After ReorderBlock the block gets repositioned by RepositionAll;
                    // we immediately override its position back to the cursor so it
                    // visually stays "in hand"
                    transform.position = eventData.position;
                }
            }
        }
    }

    // ── End drag ──────────────────────────────────────────────────────────────
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
                    dragClone.GetComponent<CanvasGroup>().blocksRaycasts = true;
                    dragClone.GetComponent<ScratchBlock>().inPanel = true;
                    panel.AddBlock(dragClone);
                }
                else
                    Destroy(dragClone);
                dragClone = null;
            }
        }
        else
        {
            canvasGroup.blocksRaycasts = true;

            if (onPanel)
            {
                inPanel = true;
                // If live-reorder already inserted us at the right index, just refresh;
                // otherwise do a clean add at the nearest index.
                if (lastInsertIndex >= 0)
                    panel.RefreshLayout(); // already in list from ReorderBlock
                else
                    panel.AddBlock(gameObject);
            }
            else
            {
                // Dropped outside panel → destroy
                Destroy(gameObject);
            }
        }

        lastInsertIndex = -1;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    bool IsOverPanel(ProgramPanel panel, PointerEventData eventData)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            panel.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera);
    }
}