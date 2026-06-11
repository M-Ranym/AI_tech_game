using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;
using UPersian.Components;

public class RoboPathUI : MonoBehaviour
{
    public static RoboPathUI Instance;

    [Header("GO Button")]
    public Button goButton;

    [Header("Success Panel (Level Complete + Final Win)")]
    public GameObject levelCompletePanel;
    public Text levelCompleteMessage;

    [Header("Wrong Answer Panel")]
    public GameObject wrongPanel;
    public Text wrongMessage;

    // Arabic strings
    private const string MSG_WIN = "احسنت! لقد اجتزت جميع المراحل";
    private const string MSG_WRONG = "اجابة خاطئة! حاول مجددا";
    private const string MSG_LEVEL_DONE = "أحسنت! انتقل إلى المرحلة التالية";

    private bool isFinalWin = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        goButton.interactable = true;
        goButton.onClick.AddListener(OnGoPressed);

        levelCompletePanel.SetActive(false);
        wrongPanel.SetActive(false);

        SetText(wrongMessage, MSG_WRONG);
    }

    public void OnLevelLoaded(int levelIndex)
    {
        wrongPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        goButton.interactable = true;
        RoboPathSlotManager.Instance.ClearAll();
    }

    public void ShowLevelComplete(int finishedLevelIndex)
    {
        wrongPanel.SetActive(false);
        levelCompletePanel.SetActive(true);
        SetText(levelCompleteMessage, MSG_LEVEL_DONE);
        isFinalWin = false;

        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RoboGoalReached");
    }

    public void ShowAllLevelsComplete()
    {
        wrongPanel.SetActive(false);
        levelCompletePanel.SetActive(true);
        SetText(levelCompleteMessage, MSG_WIN);
        isFinalWin = true;

        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RoboGoalReached");
    }

    public void ShowWrongAnswer()
    {
        levelCompletePanel.SetActive(false);
        wrongPanel.SetActive(true);

        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RoboWrongAnswer");
    }

    public void ShowWin() => ShowAllLevelsComplete();

    private void OnGoPressed()
    {
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("RoboGoButton");

        RoboPathCore.Instance.StartRun();
    }

    void Update()
    {
        // Wrong panel - click to retry
        if (wrongPanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) ||
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                wrongPanel.SetActive(false);
                RoboPathSlotManager.Instance.ClearAll();
            }
        }

        // Success panel - click to continue (except final win)
        if (levelCompletePanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) ||
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                if (!isFinalWin)
                {
                    levelCompletePanel.SetActive(false);
                    RoboPathCore.Instance.GoToNextLevel();
                }
                // Final win stays on screen
            }
        }
    }

    private void SetText(Text txt, string arabic)
    {
        if (txt == null) return;
        RtlText rtl = txt.GetComponent<RtlText>();
        if (rtl != null)
            rtl.text = arabic;
        else
            txt.text = ArabicFixer.Fix(arabic, true, false);
    }
}