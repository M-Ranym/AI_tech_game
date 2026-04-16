using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Image sadFace;
    [SerializeField] private Image normalFace;
    [SerializeField] private Image happyFace;
    [SerializeField] public float animDuration = 1.5f;

    private float _currentFill = 0f;

    void Start()
    {
        // Set to starting accuracy (30%) instantly on game start
        SnapTo(EvaluationData.accuracy / 100f);
    }

    // Snap instantly — no animation
    public void SnapTo(float normalized)
    {
        StopAllCoroutines();
        _currentFill = Mathf.Clamp01(normalized);
        if (fillImage != null)
            fillImage.fillAmount = _currentFill;
        UpdateColor();
        UpdateFace();
    }

    // Animate from old value to new value — call this from EvaluationManager
    public void AnimateFromTo(float from, float to)
    {
        StopAllCoroutines();
        StartCoroutine(DoAnimate(from, to));
    }

    IEnumerator DoAnimate(float from, float to)
    {
        from = Mathf.Clamp01(from);
        to = Mathf.Clamp01(to);

        _currentFill = from;
        if (fillImage != null)
            fillImage.fillAmount = _currentFill;
        UpdateColor();
        UpdateFace();

        yield return new WaitForEndOfFrame();

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
            _currentFill = Mathf.Lerp(from, to, t);
            if (fillImage != null)
                fillImage.fillAmount = _currentFill;
            UpdateColor();
            UpdateFace();
            yield return null;
        }

        _currentFill = to;
        if (fillImage != null)
            fillImage.fillAmount = _currentFill;
        UpdateColor();
        UpdateFace();
    }

    void UpdateColor()
    {
        if (fillImage != null)
            fillImage.color = Color.Lerp(
                new Color(0.4f, 0.8f, 1f, 1f),  // light blue (low)
                new Color(0.1f, 0.3f, 0.8f, 1f), // dark blue (high)
                _currentFill
            );
    }

    void UpdateFace()
    {
        float acc = _currentFill * 100f;
        if (sadFace != null) sadFace.gameObject.SetActive(acc < 30f);
        if (normalFace != null) normalFace.gameObject.SetActive(acc >= 30f && acc < 60f);
        if (happyFace != null) happyFace.gameObject.SetActive(acc >= 60f);
    }
}