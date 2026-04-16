using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class KidDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private string allowedTag;

    [Header("Visual States")]
    [SerializeField] private GameObject normalState;
    [SerializeField] private GameObject correctState;
    [SerializeField] private GameObject wrongState;

    [Header("Floating Area")]
    [SerializeField] private RectTransform safeArea; // 👈 NEW

    [SerializeField] private float flashDuration = 0.8f;

    private RectTransform zoneRect;
    private bool isLocked;
    private List<KidDraggable> items = new List<KidDraggable>();

    private void Awake()
    {
        zoneRect = GetComponent<RectTransform>();
        ShowNormal();
    }

    public void AddItem(KidDraggable item)
    {
        if (item == null) return;

        if (!items.Contains(item))
            items.Add(item);

        item.transform.SetParent(transform, false);

        // Use SafeArea if assigned
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

        // 👇 pass the SAFE AREA instead of full zone
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

        StartCoroutine(FlashCoroutine(eventData.pointerDrag.CompareTag(allowedTag)));
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

    private IEnumerator FlashCoroutine(bool correct)
    {
        if (correct) ShowCorrect(); else ShowWrong();
        yield return new WaitForSeconds(flashDuration);
        ShowNormal();
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
        ShowNormal();
    }

    public RectTransform GetZoneRect()
    {
        return safeArea != null ? safeArea : zoneRect;
    }

    private void ShowNormal()
    {
        if (normalState != null) normalState.SetActive(true);
        if (correctState != null) correctState.SetActive(false);
        if (wrongState != null) wrongState.SetActive(false);
    }

    private void ShowCorrect()
    {
        if (normalState != null) normalState.SetActive(false);
        if (correctState != null) correctState.SetActive(true);
        if (wrongState != null) wrongState.SetActive(false);
    }

    private void ShowWrong()
    {
        if (normalState != null) normalState.SetActive(false);
        if (correctState != null) correctState.SetActive(false);
        if (wrongState != null) wrongState.SetActive(true);
    }
}