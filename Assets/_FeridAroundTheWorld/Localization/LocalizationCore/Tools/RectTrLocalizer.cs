using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.horizon.LocalizationSystem
{
    public class RectTrLocalizer : MonoBehaviour
    {
        [SerializeField] private List<Block> BlocksList;

        public void Start()
        {
            LocalizeBlocks(); // in start cuz the localization system is not fully set when called in awake
        }

        private void LocalizeBlocks()
        {
            if (BlocksList == null || BlocksList.Count == 0) { Debug.LogError(nameof(BlocksList) + " is null or empty"); return; }

            Debug.Log("rectTr localization , parent =" + transform.parent == null ? "" : transform.parent.name);

            Block block;
            for (int i = 0; i < BlocksList.Count; i++)
            {
                block = BlocksList[i];
                if (block == null) { Debug.LogError(nameof(block) + " is null"); continue; }

                if (!block.IsValid())
                    continue;

                if (LocalizationHelper.IsRTL())
                    block.SwitchToRTL();
                else
                    block.SwitchToLTR();
            }
        }
    }
    [System.Serializable]
    public class Block
    {
        public RectTransform uiElementRectTr;
        public RectTransform RtlRectTr;//recttr for the anchor point and position
        public RectTransform LtrRectTr;

        public bool IsValid()
        {
            if (uiElementRectTr == null) { Debug.LogError(nameof(uiElementRectTr) + " is null"); return false; }
            if (RtlRectTr == null) { Debug.LogError(nameof(RtlRectTr) + " is null"); return false; }
            if (LtrRectTr == null) { Debug.LogError(nameof(LtrRectTr) + " is null"); return false; }

            return true;
        }

        public void SwitchToRTL()
        {
            if (!IsValid()) { return; }
            CopyRectTransform(RtlRectTr, uiElementRectTr);
        }
        public void SwitchToLTR()
        {
            if (!IsValid()) { return; }
            CopyRectTransform(LtrRectTr, uiElementRectTr);
        }

        private void CopyRectTransform(RectTransform src, RectTransform dst)
        {
            dst.anchorMin = src.anchorMin;
            dst.anchorMax = src.anchorMax;
            dst.anchoredPosition = src.anchoredPosition;
            dst.sizeDelta = src.sizeDelta;
            dst.pivot = src.pivot;

            dst.offsetMin = src.offsetMin;
            dst.offsetMax = src.offsetMax;

            dst.localRotation = src.localRotation;
            dst.localScale = src.localScale;
        }
    }
}