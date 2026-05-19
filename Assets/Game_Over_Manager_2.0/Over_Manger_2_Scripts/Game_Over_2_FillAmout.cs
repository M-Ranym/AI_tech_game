using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_Over_2_FillAmout : MonoBehaviour
{
    [SerializeField] private float _duration = 10f;

    public void Fill_Amont(Image img, float currentValue, float maxValue)
    {
        float target = currentValue / maxValue;
        StartCoroutine(Fill_Amont_Animation(img, target));
    }

    private IEnumerator Fill_Amont_Animation(Image filledImage, float target)
    {
        float t = 0;
        float currentFillValue = filledImage.fillAmount;

        while (t <= _duration)
        {
            t += Time.unscaledDeltaTime;
            filledImage.fillAmount = Mathf.Lerp(currentFillValue, target, t / _duration);
            yield return null;
        }
    }
}