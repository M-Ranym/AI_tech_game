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

    // For toolbox drag: we create a clone that follows the cursor
    private GameObject dragClone = null;

    // For panel drag: we lift the real object and follow the cursor
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

    // ── Begin drag ────────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        lastInsertIndex = -1;

        if (isInToolbox)
        {
            // Create a clone at the canvas root that follows the cursor.
            // The original stays in the toolbox so the user can drag again.
            dragClone = Instantiate(gameObject, canvas.transform, true);
            dragClone.transform.position = eventData.position;

            var cs = dragClone.GetComponent<ScratchBlock>();
            cs.isInToolbox = false;
            cs.inPanel = false;
            cs.spriteTop = spriteTop;
            cs.spriteMiddle = spriteMiddle;
            cs.spriteBottom = spriteBottom;

            // Clone must not block raycasts so the panel can detect drop
            dragClone.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else if (inPanel)
        {
            // Lift ourselves out of the panel list.
            // We stay visible — just follow the cursor directly.
            ProgramPanel panel = FindObjectOfType<ProgramPanel>();
            if (panel != null)
            {
                panel.RemoveBlock(gameObject);
                panel.SetDragging(gameObject);
            }

            inPanel = false;

            // Render on top of everything while dragging
            transform.SetParent(canvas.transform, false);

            // 🔥 FORCE Z = 0
            rectTransform.anchoredPosition3D = new Vector3(
                rectTransform.anchoredPosition3D.x,
                rectTransform.anchoredPosition3D.y,
                0f
            );

            // Keep on top
            transform.SetAsLastSibling();

            // 🔥 FORCE correct UI position
            Vector3 pos = transform.localPosition;
            pos.z = 0f;
            transform.localPosition = pos;

            // Also make sure scale is normal
            transform.localScale = Vector3.one;

            // Keep it on top
            transform.SetAsLastSibling();

            // 🔥 FORCE IT TO BE ON TOP
            transform.SetAsLastSibling();

            canvasGroup.blocksRaycasts = false;

            // Keep it visible while dragging
            canvasGroup.alpha = 0.9f;
        }
    }

    // ── During drag ───────────────────────────────────────────────────────────
    public void OnDrag(PointerEventData eventData)
    {
        if (isInToolbox)
        {
            // Move the clone with the cursor
            if (dragClone != null)
                dragClone.transform.position = eventData.position;
        }
        else
        {
            // Move ourselves with the cursor — always visible
            transform.position = eventData.position;

            // Live reorder: push other blocks out of the way smoothly
            ProgramPanel panel = FindObjectOfType<ProgramPanel>();
            if (panel != null && IsOverPanel(panel, eventData))
            {
                int idx = panel.GetInsertIndex(eventData.position.y);
                if (idx != lastInsertIndex)
                {
                    lastInsertIndex = idx;
                    // Tell panel to rearrange others into positions as if we were at idx.
                    // We are NOT in the blocks list right now, so ReorderBlock adds us,
                    // computes targets for everyone, then we override our own position.
                    panel.ReorderBlock(gameObject, idx);
                    // Immediately put ourselves back under the cursor
                    transform.position = eventData.position;
                    rectTransform.anchoredPosition3D = new Vector3(
    rectTransform.anchoredPosition3D.x,
    rectTransform.anchoredPosition3D.y,
    0f
);
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
                    var cloneSB = dragClone.GetComponent<ScratchBlock>();
                    cloneSB.inPanel = true;
                    // Re-parent into panel so it gets managed correctly
                    dragClone.transform.SetParent(panel.transform, true);
                    panel.AddBlock(dragClone);
                }
                else
                    Destroy(dragClone);

                dragClone = null;
            }
        }
        else
        {
            // Restore alpha and raycasts
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            panel?.ClearDragging();

            if (onPanel)
            {
                inPanel = true;
                // Re-parent back into panel
                transform.SetParent(panel.transform, true);

                if (lastInsertIndex >= 0)
                {
                    // Already at the right index from live preview — snap to target
                    panel.RefreshLayout();
                    panel.SnapToTargets();
                }
                else
                {
                    panel.AddBlock(gameObject);
                }
            }
            else
            {
                // Dropped outside panel — destroy
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