using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KidDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private string allowedTag;

    [Header("Floating Area")]
    [SerializeField] private RectTransform safeArea;

    [SerializeField] private float flashDuration = 0.8f;

    private RectTransform zoneRect;
    private bool isLocked;
    private List<KidDraggable> items = new List<KidDraggable>();

    private Image boxImage;
    private Color originalColor;

    private void Awake()
    {
        zoneRect = GetComponent<RectTransform>();

        boxImage = GetComponent<Image>();
        if (boxImage == null)
            boxImage = GetComponentInChildren<Image>();

        if (boxImage != null)
            originalColor = boxImage.color;
        else
            Debug.LogError($"[KidDropZone] NO Image found on {gameObject.name}!");
    }

    public void AddItem(KidDraggable item)
    {
        if (item == null) return;

        if (!items.Contains(item))
            items.Add(item);

        item.transform.SetParent(transform, false);

        RectTransform targetArea = safeArea != null ? safeArea : zoneRect;

        RectTransform itemRect = item.GetComponent<RectTransform>();
        if (itemRect != null)
        {
            float halfW = Mathf.Max(0, targetArea.rect.width / 2f - itemRect.rect.width / 2f - 10f);
            float halfH = Mathf.Max(0, targetArea.rect.height / 2f - itemRect.rect.height / 2f - 10f);

            itemRect.anchoredPosition = new Vector2(
                Random.Range(-halfW, halfW),
                Random.Range(-halfH, halfH)
            );
        }

        item.StartFloatingInZone(targetArea);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isLocked) return;
        if (eventData == null || eventData.pointerDrag == null) return;

        KidDraggable draggable = eventData.pointerDrag.GetComponent<KidDraggable>();
        if (draggable == null) return;

        foreach (KidDropZone zone in FindObjectsOfType<KidDropZone>())
            zone.RemoveItem(draggable);

        AddItem(draggable);
        bool correct = eventData.pointerDrag.CompareTag(allowedTag);

        // Notify the tutorial manager if a tutorial is running
        KidTutorialManager tm = FindObjectOfType<KidTutorialManager>();
        if (tm != null && tm.IsTutorialActive())
            tm.OnTutorialDrop(eventData.pointerDrag, correct);

        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx(correct ? "KidDropCorrect" : "KidDropWrong");

        StartCoroutine(FlashCoroutine(correct));
    }

    public void RemoveItem(KidDraggable item)
    {
        items.Remove(item);
    }

    public bool ValidateItems()
    {
        if (items.Count == 0)
        {
            StartCoroutine(FlashCoroutine(false));
            return false;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) continue;
            if (!items[i].CompareTag(allowedTag))
            {
                StartCoroutine(FlashCoroutine(false));
                return false;
            }
        }

        StartCoroutine(FlashCoroutine(true));
        LockZone();
        return true;
    }

    // ==================== Tutorial Flash ====================

    /// <summary>
    /// Called by KidTutorialManager to flash the zone colour during tutorial demos.
    /// After calling this, the caller should yield for flashDuration seconds
    /// and then call ResetFlash() to restore the original colour.
    /// </summary>
    public IEnumerator TutorialFlash(bool correct)
    {
        if (boxImage == null) yield break;
        Color original = boxImage.color;
        Color flashColor = correct ? Color.green : Color.red;
        flashColor.a = original.a;

        for (int i = 0; i < 3; i++)
        {
            boxImage.color = flashColor;
            yield return new WaitForSeconds(0.3f);
            boxImage.color = original;
            yield return new WaitForSeconds(0.2f);
        }
        boxImage.color = original;
    }

    /// <summary>
    /// Immediately restores the zone image to its original colour.
    /// Called by KidTutorialManager after FlashAndWait completes.
    /// </summary>
    public void ResetFlash()
    {
        if (boxImage != null)
            boxImage.color = originalColor;
    }

    // ========================================================

    private IEnumerator FlashCoroutine(bool correct)
    {
        if (boxImage != null)
            boxImage.color = correct ? Color.green : Color.red;

        yield return new WaitForSeconds(flashDuration);

        if (boxImage != null)
            boxImage.color = originalColor;
    }

    private void LockZone()
    {
        isLocked = true;
        for (int i = 0; i < items.Count; i++)
            if (items[i] != null) items[i].LockDragging();
    }

    public void ResetZone()
    {
        isLocked = false;
        items.Clear();

        if (boxImage != null)
            boxImage.color = originalColor;
    }

    public RectTransform GetZoneRect()
    {
        return safeArea != null ? safeArea : zoneRect;
    }
}