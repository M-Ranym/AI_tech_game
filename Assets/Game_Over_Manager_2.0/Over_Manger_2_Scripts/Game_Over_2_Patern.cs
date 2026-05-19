using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_Over_2_Patern : MonoBehaviour
{
    private RawImage _imagePattern;
    [SerializeField] private float _x, _y;

    private bool _startAnim = true;
    [SerializeField] private float _animDelay = 1f;

    private void Start()
    {
        _imagePattern = GetComponent<RawImage>();
    }

    private IEnumerator Pattern_Animation(float delay)
    {
        yield return new WaitForSeconds(delay);
        _imagePattern.uvRect = new Rect(_imagePattern.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, _imagePattern.uvRect.size);
    }

    private void Update()
    {
        if (_startAnim) StartCoroutine(Pattern_Animation(_animDelay));
    }
}