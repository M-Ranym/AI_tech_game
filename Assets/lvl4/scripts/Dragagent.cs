using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAgent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector3 startPosition;
    private RectTransform trashRect;
    private RectTransform trashUpperRect;
    private RectTransform trashLowerRect;
    private Image shadowImage;
    private Image agentImage;
    private Color originalColor;
    private Color glowColor;


    private float randomOffset;
    private float randomSpeed;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        agentImage = GetComponent<Image>();
        originalColor = agentImage.color;
        startPosition = rectTransform.anchoredPosition;

        randomOffset = Random.Range(0f, 10f);
        randomSpeed = Random.Range(1.5f, 3.5f);

        AgentManager manager = FindObjectOfType<AgentManager>();
        if (manager != null)
        {
            Transform trash = manager.GetTrashCan();
            Transform upper = manager.GetTrashUpper();
            Transform lower = manager.GetTrashLower();

            if (trash != null)
                trashRect = trash.GetComponent<RectTransform>();
            if (upper != null)
                trashUpperRect = upper.GetComponent<RectTransform>();
            if (lower != null)
                trashLowerRect = lower.GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        PulseColor();
    }

    void PulseColor()
    {
        if (agentImage != null && glowColor != Color.white)
        {
            float pulse = Mathf.Sin((Time.time + randomOffset) * randomSpeed) * 0.5f + 0.5f;
            Color lerpedColor = Color.Lerp(originalColor, glowColor, pulse);
            agentImage.color = lerpedColor;
        }
    }

    public void SetShadowImage(Image shadow)
    {
        shadowImage = shadow;
    }

    public void SetGlowColor(Color color)
    {
        glowColor = color;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        if (shadowImage != null)
            shadowImage.gameObject.SetActive(true);
        if (Game_Over_2_AudioManager.audioManInstance != null)
            Game_Over_2_AudioManager.audioManInstance.Play_Sfx("AgentDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;

        if (trashRect != null && trashUpperRect != null && trashLowerRect != null)
        {
            Vector3 agentPos = rectTransform.position;
            Vector3 upperPos = trashUpperRect.position;

            if (agentPos.y > upperPos.y)
                rectTransform.SetAsLastSibling();
            else
                rectTransform.SetAsFirstSibling();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (trashRect == null)
        {
            rectTransform.anchoredPosition = startPosition;
            return;
        }

        float distToTrash = Vector3.Distance(rectTransform.position, trashRect.position);

        if (distToTrash < 150)
        {
            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("AgentDropTrash");
            gameObject.SetActive(false);
        }
        else
        {
            if (Game_Over_2_AudioManager.audioManInstance != null)
                Game_Over_2_AudioManager.audioManInstance.Play_Sfx("AgentDropReturn");
            rectTransform.anchoredPosition = startPosition;
        }
    }

    public void HideShadow()
    {
        if (shadowImage != null)
            shadowImage.gameObject.SetActive(false);
    }
}