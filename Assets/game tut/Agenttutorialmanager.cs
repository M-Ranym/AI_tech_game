using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;

public class AgentTutorialManager : MonoBehaviour
{
    [Header("Ferid & Speech Bubble")]
    public RectTransform feridRect;
    public RectTransform speechBubbleRect;
    public RtlText speechText;

    [Header("Audio")]
    public Game_Over_2_AudioManager audioManager;

    [Header("Hand")]
    public GameObject tutorialHand;
    public float handBobSpeed = 2.0f;
    public float handBobAmount = 12f;
    public float handSquashSpeed = 3.0f;

    [Header("Overlay")]
    public GameObject darkOverlay;

    [Header("Agents (same objects AgentManager uses)")]
    public Transform agentsParent;
    public Image goodDemoAgent;
    public Image badAgent1;
    public Image badAgent2;
    public Image badAgent3;
    public Sprite goodAgentSprite;
    public Sprite badAgentSprite;
    public Vector2 goodAgentSize = new Vector2(229.6f, 248.0f);
    public Vector2 badAgentSize = new Vector2(338.0f, 469.3f);

    [Header("Trash & Submit")]
    public Transform trashCan;
    
    
    
    
    
    public Transform trashUpper;
    public Transform trashLower;
    public Button submitButton;

    private const string V1 = "AgentTutorial_Step1"; 
    private const string V2 = "AgentTutorial_Step2"; 
    private const string V3 = "AgentTutorial_Step3"; 
    private const string V4 = "AgentTutorial_Step4"; 
    private const string V5 = "AgentTutorial_Step5"; 
    private const string V6 = "AgentTutorial_Step6"; 

    private GameFinishTrigger _trigger;
    private bool _tutorialFinished = false;
    private bool _submitClicked = false;

    private Vector2 _handBasePos;
    private float _handBaseRot;
    private float _handTimer = 0f;
    private Coroutine _handCoroutine = null;

    private void Start()
    {
        _trigger = FindObjectOfType<GameFinishTrigger>();

        
        
        if (_trigger != null && _trigger.GetStartLevel() != 0)
        {
            enabled = false;
            return;
        }

        if (audioManager == null)
            audioManager = FindObjectOfType<Game_Over_2_AudioManager>();

        EnsureHandNonBlocking();
        if (tutorialHand != null) tutorialHand.SetActive(false);
        if (speechBubbleRect != null) speechBubbleRect.gameObject.SetActive(false);

        SetupTutorialAgents();
        SetOneInteractable(submitButton.gameObject, false);

        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        BringInFrontOfOverlay(new List<GameObject>

        
        
        
        