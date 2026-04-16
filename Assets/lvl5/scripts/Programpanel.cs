using UnityEngine;
using System.Collections.Generic;

public class ProgramPanel : MonoBehaviour
{
    [Tooltip("Gap between blocks in canvas units.")]
    public float spacing = 2f;

    [Tooltip("How many block-heights to skip from the top before the first block.")]
    public float topPaddingBlocks = 2f;

    private List<GameObject> blocks = new List<GameObject>();
    private RectTransform panelRect;

    // The natural (un-shrunk) world height of one block, measured on first drop
    private float naturalBlockWorldH = -1f;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        Destroy(GetComponent<UnityEngine.UI.VerticalLayoutGroup>());
        Destroy(GetComponent<UnityEngine.UI.HorizontalLayoutGroup>());
        Destroy(GetComponent<UnityEngine.UI.ContentSizeFitter>());
    }

    public void AddBlock(GameObject block)
    {
        if (!blocks.Contains(block))
            blocks.Add(block);
        RepositionAll();
    }

    public void RemoveBlock(GameObject block)
    {
        blocks.Remove(block);
        RepositionAll();
    }

    // ── Called by ScratchBlock while dragging inside the panel ────────────────
    // Returns the index the dragged block would be inserted at given the current
    // cursor world-Y position, so ScratchBlock can show a live preview.
    public int GetInsertIndex(float worldY)
    {
        blocks.RemoveAll(b => b == null);
        if (blocks.Count == 0) return 0;

        for (int i = 0; i < blocks.Count; i++)
        {
            float blockCentreY = blocks[i].transform.position.y;
            if (worldY > blockCentreY) return i;
        }
        return blocks.Count;
    }

    // ── Move a block already in the panel to a new index (live reorder) ───────
    public void ReorderBlock(GameObject block, int newIndex)
    {
        blocks.Remove(block);
        newIndex = Mathf.Clamp(newIndex, 0, blocks.Count);
        blocks.Insert(newIndex, block);
        RepositionAll();
    }

    void RepositionAll()
    {
        blocks.RemoveAll(b => b == null);
        if (blocks.Count == 0) return;

        // ── Panel bounds ──────────────────────────────────────────────────────
        Vector3[] panelCorners = new Vector3[4];
        panelRect.GetWorldCorners(panelCorners);
        // corners: 0=BL 1=TL 2=TR 3=BR
        Vector3 topLeft = panelCorners[1];
        float panelWorldH = panelCorners[1].y - panelCorners[0].y; // TL.y - BL.y
        float panelWorldW = panelCorners[2].x - panelCorners[1].x;

        Canvas canvas = GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.transform.localScale.y : 1f;
        float worldSpacing = spacing * scale;

        // ── Measure natural block height on first use ──────────────────────────
        if (naturalBlockWorldH < 0f)
        {
            Vector3[] bc = new Vector3[4];
            blocks[0].GetComponent<RectTransform>().GetWorldCorners(bc);
            naturalBlockWorldH = Mathf.Abs(bc[1].y - bc[0].y);
        }

        float blockH = naturalBlockWorldH;

        // ── How much vertical space is available for blocks ───────────────────
        float topOffset = blockH * topPaddingBlocks;
        float availableH = panelWorldH - topOffset - worldSpacing;

        // Total height needed if we use natural block size
        float totalNeeded = blocks.Count * blockH + (blocks.Count - 1) * worldSpacing;

        // If blocks overflow, scale blockH down so everything fits
        if (totalNeeded > availableH && blocks.Count > 0)
        {
            // Solve: count*h + (count-1)*spacing = availableH  →  h = (availableH - (count-1)*spacing) / count
            float shrunkH = (availableH - (blocks.Count - 1) * worldSpacing) / blocks.Count;
            blockH = Mathf.Max(shrunkH, 4f * scale); // never collapse to nothing
        }

        // ── Position each block ───────────────────────────────────────────────
        for (int i = 0; i < blocks.Count; i++)
        {
            RectTransform rt = blocks[i].GetComponent<RectTransform>();

            // Shrink the RectTransform height to match the (possibly reduced) blockH
            // Width stays as-is (fills panel width naturally)
            float blockCanvasH = blockH / scale;
            Vector2 sd = rt.sizeDelta;
            sd.y = blockCanvasH;
            rt.sizeDelta = sd;

            float x = topLeft.x + panelWorldW * 0.5f;
            float y = topLeft.y
                      - topOffset
                      - (blockH * 0.5f)
                      - i * (blockH + worldSpacing);

            blocks[i].transform.position = new Vector3(x, y, topLeft.z);

            ScratchBlock sb = blocks[i].GetComponent<ScratchBlock>();
            if (sb != null) sb.SetPuzzleShape(i, blocks.Count);
        }
    }

    public void RefreshLayout() => RepositionAll();

    public List<BlockType> GetProgram()
    {
        blocks.RemoveAll(b => b == null);
        var program = new List<BlockType>();
        foreach (GameObject block in blocks)
        {
            ScratchBlock sb = block.GetComponent<ScratchBlock>();
            if (sb != null) program.Add(sb.blockType);
        }
        return program;
    }

    public void ClearProgram()
    {
        foreach (GameObject block in blocks)
            if (block != null) Destroy(block);
        blocks.Clear();
        naturalBlockWorldH = -1f; // reset so next level re-measures
    }

    public void SoftReset() { }
}