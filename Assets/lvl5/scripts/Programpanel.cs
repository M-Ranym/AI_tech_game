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

    // Natural block world-height, measured once from the first block dropped
    private float naturalBlockWorldH = -1f;

    // Cached target world positions per block for smooth lerping
    private Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();

    // The block currently being dragged (excluded from smooth positioning)
    private GameObject draggingBlock = null;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        Destroy(GetComponent<UnityEngine.UI.VerticalLayoutGroup>());
        Destroy(GetComponent<UnityEngine.UI.HorizontalLayoutGroup>());
        Destroy(GetComponent<UnityEngine.UI.ContentSizeFitter>());
    }

    void Update()
    {
        // Each frame, smoothly lerp every block (except the one being dragged)
        // toward its target position
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

    // ── Called by ScratchBlock to set which block is being dragged ────────────
    public void SetDragging(GameObject block) => draggingBlock = block;
    public void ClearDragging() => draggingBlock = null;

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

    // Returns which slot index the cursor Y maps to
    public int GetInsertIndex(float worldY)
    {
        blocks.RemoveAll(b => b == null);
        for (int i = 0; i < blocks.Count; i++)
        {
            if (!targetPositions.ContainsKey(blocks[i])) continue;
            if (worldY > targetPositions[blocks[i]].y) return i;
        }
        return blocks.Count;
    }

    // Reorder a block to a new index, update targets (others will slide smoothly)
    public void ReorderBlock(GameObject block, int newIndex)
    {
        if (!blocks.Contains(block)) blocks.Add(block);
        blocks.Remove(block);
        newIndex = Mathf.Clamp(newIndex, 0, blocks.Count);
        blocks.Insert(newIndex, block);
        ComputeTargets();
    }

    // ── Core: compute where every block SHOULD be, store as targets ──────────
    void ComputeTargets()
    {
        blocks.RemoveAll(b => b == null);
        if (blocks.Count == 0) return;

        Vector3[] panelCorners = new Vector3[4];
        panelRect.GetWorldCorners(panelCorners);
        // 0=BL 1=TL 2=TR 3=BR
        Vector3 topLeft = panelCorners[1];
        float panelWorldH = panelCorners[1].y - panelCorners[0].y;
        float panelWorldW = panelCorners[2].x - panelCorners[1].x;

        Canvas canvas = GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.transform.localScale.y : 1f;
        float wSpacing = spacing * scale;
        float wPad = verticalPaddingCanvas * scale; // same top AND bottom

        // Measure natural block height once
        if (naturalBlockWorldH < 0f)
        {
            Vector3[] bc = new Vector3[4];
            blocks[0].GetComponent<RectTransform>().GetWorldCorners(bc);
            naturalBlockWorldH = Mathf.Abs(bc[1].y - bc[0].y);
        }

        float blockH = naturalBlockWorldH;

        // Available height between top-pad and bottom-pad (equal on both sides)
        float availH = panelWorldH - wPad * 2f;

        // Total height needed at natural size
        float totalNeeded = blocks.Count * blockH + (blocks.Count - 1) * wSpacing;

        // Shrink if needed so everything fits inside the padded area
        if (totalNeeded > availH && blocks.Count > 0)
        {
            float shrunk = (availH - (blocks.Count - 1) * wSpacing) / blocks.Count;
            blockH = Mathf.Max(shrunk, 4f * scale);
        }

        // Apply the (possibly shrunk) height to each block's RectTransform
        float blockCanvasH = blockH / scale;
        foreach (var b in blocks)
        {
            if (b == null) continue;
            var rt = b.GetComponent<RectTransform>();
            var sd = rt.sizeDelta;
            sd.y = blockCanvasH;
            rt.sizeDelta = sd;
        }

        // Compute total occupied height and centre it vertically within the padded area
        float totalOccupied = blocks.Count * blockH + (blocks.Count - 1) * wSpacing;
        // Top of the first block, centred between top-pad and bottom-pad
        float firstBlockTop = topLeft.y - wPad - (availH - totalOccupied) * 0.5f;

        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i] == null) continue;

            float x = topLeft.x + panelWorldW * 0.5f;
            float y = firstBlockTop - blockH * 0.5f - i * (blockH + wSpacing);

            Vector3 target = new Vector3(x, y, topLeft.z);
            targetPositions[blocks[i]] = target;

            ScratchBlock sb = blocks[i].GetComponent<ScratchBlock>();
            if (sb != null) sb.SetPuzzleShape(i, blocks.Count);
        }
    }

    // Instantly snap all blocks to their targets (used on drop)
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

    public List<BlockType> GetProgram()
    {
        blocks.RemoveAll(b => b == null);
        var program = new List<BlockType>();
        foreach (var block in blocks)
        {
            ScratchBlock sb = block.GetComponent<ScratchBlock>();
            if (sb != null) program.Add(sb.blockType);
        }
        return program;
    }

    public void ClearProgram()
    {
        foreach (var block in blocks)
            if (block != null) Destroy(block);
        blocks.Clear();
        targetPositions.Clear();
        naturalBlockWorldH = -1f;
        draggingBlock = null;
    }

    public void SoftReset() { }
}