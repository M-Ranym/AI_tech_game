using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProgramPanel : MonoBehaviour
{
    [Tooltip("Gap between blocks in canvas units.")]
    public float spacing = 2f;

    [Tooltip("Padding in canvas units applied equally to top AND bottom.")]
    public float verticalPaddingCanvas = 20f;

    [Tooltip("Speed at which blocks slide to their target positions (higher = faster).")]
    public float slideSpeed = 12f;

    private List<GameObject> blocks = new List<GameObject>();
    private RectTransform panelRect;

    private float naturalBlockWorldH = -1f;

    private Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();

    private GameObject draggingBlock = null;

    // 🔥 NEW (for Excel-like behavior)
    private int insertIndex = -1;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();

        Destroy(GetComponent<UnityEngine.UI.VerticalLayoutGroup>());
        Destroy(GetComponent<UnityEngine.UI.HorizontalLayoutGroup>());
        Destroy(GetComponent<UnityEngine.UI.ContentSizeFitter>());
    }

    void Update()
    {
        foreach (var block in blocks)
        {
            if (block == null) continue;
            if (block == draggingBlock) continue;
            if (!targetPositions.ContainsKey(block)) continue;

            block.transform.position = Vector3.Lerp(
                block.transform.position,
                targetPositions[block],
                Time.deltaTime * slideSpeed
            );
        }
    }

    public void SetDragging(GameObject block) => draggingBlock = block;

    public void ClearDragging()
    {
        draggingBlock = null;
        insertIndex = -1;
    }

    // 🔥 NEW
    public void SetInsertIndex(int idx)
    {
        insertIndex = idx;
        ComputeTargets();
    }

    public void AddBlock(GameObject block)
    {
        if (!blocks.Contains(block))
            blocks.Add(block);

        ComputeTargets();
    }

    public void RemoveBlock(GameObject block)
    {
        blocks.Remove(block);
        targetPositions.Remove(block);
        ComputeTargets();
    }

    public int GetInsertIndex(float worldY)
    {
        blocks.RemoveAll(b => b == null);

        int visualIndex = 0;

        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i] == draggingBlock)
                continue;

            if (!targetPositions.ContainsKey(blocks[i])) continue;

            float targetY = targetPositions[blocks[i]].y;

            // 🔥 BIG FIX: use a "zone" instead of exact line
            float threshold = 80f; // you can tweak this (20–60)

            if (worldY > targetY - threshold)
                return visualIndex;

            visualIndex++;
        }

        return visualIndex;
    }

    // 🔥 FINAL reorder ONLY on drop
    public void FinalizeReorder(GameObject block)
    {
        blocks.Remove(block);

        int idx = Mathf.Clamp(insertIndex, 0, blocks.Count);
        blocks.Insert(idx, block);

        insertIndex = -1;

        ComputeTargets();
    }

    void ComputeTargets()
    {
        blocks.RemoveAll(b => b == null);
        if (blocks.Count == 0) return;

        Vector3[] panelCorners = new Vector3[4];
        panelRect.GetWorldCorners(panelCorners);

        Vector3 topLeft = panelCorners[1];
        float panelWorldH = panelCorners[1].y - panelCorners[0].y;
        float panelWorldW = panelCorners[2].x - panelCorners[1].x;

        Canvas canvas = GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.transform.localScale.y : 1f;

        float wSpacing = spacing * scale;
        float wPad = verticalPaddingCanvas * scale;

        if (naturalBlockWorldH < 0f)
        {
            Vector3[] bc = new Vector3[4];
            blocks[0].GetComponent<RectTransform>().GetWorldCorners(bc);
            naturalBlockWorldH = Mathf.Abs(bc[1].y - bc[0].y);
        }

        float blockH = naturalBlockWorldH;

        float availH = panelWorldH - wPad * 2f;
        float totalNeeded = blocks.Count * blockH + (blocks.Count - 1) * wSpacing;

        if (totalNeeded > availH && blocks.Count > 0)
        {
            float shrunk = (availH - (blocks.Count - 1) * wSpacing) / blocks.Count;
            blockH = Mathf.Max(shrunk, 4f * scale);
        }

        float blockCanvasH = blockH / scale;

        foreach (var b in blocks)
        {
            if (b == null) continue;

            var rt = b.GetComponent<RectTransform>();
            var sd = rt.sizeDelta;
            sd.y = blockCanvasH;
            rt.sizeDelta = sd;
        }

        float totalOccupied = blocks.Count * blockH + (blocks.Count - 1) * wSpacing;
        float firstBlockTop = topLeft.y - wPad - (availH - totalOccupied) * 0.5f;

        int visualIndex = 0;

        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i] == null) continue;

            if (blocks[i] == draggingBlock)
                continue;

            float x = topLeft.x + panelWorldW * 0.5f;

            float y = firstBlockTop - blockH * 0.5f - visualIndex * (blockH + wSpacing);

            // 🔥 THIS is the Excel gap
            if (draggingBlock != null && insertIndex >= 0 && visualIndex >= insertIndex)
            {
                y -= (blockH + wSpacing) * 1.2f;
            }

            Vector3 target = new Vector3(x, y, topLeft.z);
            targetPositions[blocks[i]] = target;

            ScratchBlock sb = blocks[i].GetComponent<ScratchBlock>();
            if (sb != null) sb.SetPuzzleShape(i, blocks.Count);

            visualIndex++;
        }
    }

    public void SnapToTargets()
    {
        foreach (var block in blocks)
        {
            if (block == null || block == draggingBlock) continue;

            if (targetPositions.ContainsKey(block))
                block.transform.position = targetPositions[block];
        }
    }

    public void RefreshLayout() => ComputeTargets();

    // ✅ RESTORED (your MazeGenerator needs this)
    public List<BlockType> GetProgram()
    {
        blocks.RemoveAll(b => b == null);

        var program = new List<BlockType>();

        foreach (var block in blocks)
        {
            ScratchBlock sb = block.GetComponent<ScratchBlock>();
            if (sb != null)
                program.Add(sb.blockType);
        }

        return program;
    }

    // ✅ RESTORED
    public void ClearProgram()
    {
        foreach (var block in blocks)
        {
            if (block != null)
                Destroy(block);
        }

        blocks.Clear();
        targetPositions.Clear();
        naturalBlockWorldH = -1f;
        draggingBlock = null;
        insertIndex = -1;
    }

    public void SoftReset() { }
}