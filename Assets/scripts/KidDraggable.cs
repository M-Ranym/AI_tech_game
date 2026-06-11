using UnityEngine;
using UnityEngine.EventSystems;

public class KidDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError($"{gameObject.name}: No Canvas found in parents");

        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData == null) return;

        originalParent = transform.parent;
        StopFloating();

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("KidDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData == null) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // If not caught by any drop zone, return to original parent and float there
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent, false);

            KidDropZone zone = originalParent.GetComponent<KidDropZone>();
            if (zone != null)
                StartFloatingInZone(zone.GetZoneRect());
        }
    }

    public void StartFloatingInZone(RectTransform zoneRect)
    {
        if (zoneRect == null)
        {
            Debug.LogError($"{gameObject.name}: zoneRect is null");
            return;
        }

        KidItemAnimator animator = GetComponent<KidItemAnimator>();
        if (animator == null)
            animator = gameObject.AddComponent<KidItemAnimator>();

        animator.StartMoving(zoneRect);
    }

    private void StopFloating()
    {
        KidItemAnimator animator = GetComponent<KidItemAnimator>();
        if (animator != null)
            animator.StopMoving();
    }

    public void LockDragging()
    {
        enabled = false;
        StopFloating();
    }

    public void UnlockDragging()
    {
        enabled = true;
    }
}