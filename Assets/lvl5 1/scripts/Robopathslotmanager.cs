using System.Collections;
using UnityEngine;

public class RoboPathSlotManager : MonoBehaviour
{
    public static RoboPathSlotManager Instance;

    [Header("Current Level Slots ONLY")]
    private RoboPathDropSlot[] slots;

    [Header("Memory Scan Colors")]
    public Color scanColor = Color.yellow;
    public Color foundColor = Color.green;
    public Color wrongColor = Color.red;

    [Header("Scan Speed")]
    public float scanDelay = 1.2f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ✅ NEW: Set slots per level
    public void SetSlots(RoboPathDropSlot[] levelSlots)
    {
        slots = levelSlots;

        for (int i = 0; i < slots.Length; i++)
            slots[i].gameObject.SetActive(true);
    }

    // ── Drop / Lift ──────────────────────────────
    public void HandleDrop(RoboPathDragImage card, Vector2 dropScreenPos)
    {
        int nearest = GetNearestSlotIndex(dropScreenPos);
        slots[nearest].PlaceCard(card);
        card.MarkAsPlaced();
    }

    public void HandleLift(RoboPathDragImage card)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].PlacedCard == card)
            {
                slots[i].LiftCard();
                return;
            }
        }
    }

    // ── Scan ─────────────────────────────────────
    public IEnumerator ScanForAction(
     RoboPathCore.RobotAction needed,
     System.Action<int> onDone)
    {
        int foundIndex = -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsFilled) continue;

            slots[i].SetCardColor(scanColor);

            // Sound for scanning each card
            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RoboScanCard");

            yield return new WaitForSeconds(scanDelay);

            if (slots[i].PlacedAction == needed)
            {
                foundIndex = i;
                slots[i].SetCardColor(foundColor);

                // Correct card found sound
                if (Game_Over_2_AudioManager.audioManInstance != null)
                    Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RoboCardFound");

                break;
            }
            else
            {
                slots[i].ResetCardColor();
            }
        }

        if (foundIndex == -1)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsFilled &&
                    slots[i].PlacedAction == RoboPathCore.RobotAction.None)
                {
                    slots[i].SetCardColor(wrongColor);
                }
            }

            yield return new WaitForSeconds(1.5f);
        }

        onDone?.Invoke(foundIndex);
    }
    // ── Helpers ──────────────────────────────────
    public void ResetFoundSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
            slots[slotIndex].ResetCardColor();
    }

    public void ResetAllColors()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].ResetCardColor();
    }

    public RoboPathCore.RobotAction[] GetChoices()
    {
        var choices = new RoboPathCore.RobotAction[slots.Length];
        for (int i = 0; i < slots.Length; i++)
            choices[i] = slots[i].PlacedAction;
        return choices;
    }

    public void ClearAll()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].RemoveCard();
    }

    public void MarkWrong(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
            slots[slotIndex].SetCardColor(wrongColor);
    }

    private int GetNearestSlotIndex(Vector2 screenPos)
    {
        int nearest = 0;
        float minDist = float.MaxValue;

        for (int i = 0; i < slots.Length; i++)
        {
            Vector3[] corners = new Vector3[4];
            slots[i].GetComponent<RectTransform>().GetWorldCorners(corners);
            Vector3 center = (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;

            Vector2 slotScreen = RectTransformUtility.WorldToScreenPoint(null, center);
            float dist = Vector2.Distance(screenPos, slotScreen);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = i;
            }
        }

        return nearest;
    }
}