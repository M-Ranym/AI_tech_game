using UnityEngine;
using UPersian.Components;

public class UIManager : MonoBehaviour
{
    [Header("Play/Reset Button")]
    public RtlText playResetButtonLabel;

    [Header("Result Panel")]
    public GameObject resultPanel;

    [Header("Drag Hint (optional)")]
    public RtlText dragHintLabel;

    void Start()
    {
        SetRTL(playResetButtonLabel, "العب الان");
        SetRTL(dragHintLabel, "اضف البطاقات هنا");
        if (resultPanel) resultPanel.SetActive(false);
    }

    void SetRTL(RtlText label, string text)
    {
        if (label == null) return;

        label.text = text;
    }

    // ── Public label setters ─────────────────────────────

    public void SetPlayLabel() => SetRTL(playResetButtonLabel, "العب الان");

    public void SetStopLabel() => SetRTL(playResetButtonLabel, "ايقاف");

    public void SetRepeatLabel() => SetRTL(playResetButtonLabel, "اعادة");

    // ── Button pressed ───────────────────────────────────

    public void OnPlayResetPressed()
    {
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("MazeButton");

        MazeGenerator maze = FindObjectOfType<MazeGenerator>();
        if (maze == null) return;
        maze.OnPlayResetPressed();
    }
}