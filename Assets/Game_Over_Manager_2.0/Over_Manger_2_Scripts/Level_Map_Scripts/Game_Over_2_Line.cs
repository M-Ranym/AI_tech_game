using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class Game_Over_2_Line : MonoBehaviour
{
    [SerializeField] private RawImage _line;
    [SerializeField] private Vector2 _offset;

    private void Update()
    {
        if (_line.uvRect.position.x < 0.337f)
        {
            _line.uvRect = new Rect(_line.uvRect.position + _offset * Time.deltaTime, _line.uvRect.size);
        }
        else
        {
            _line.uvRect = new Rect(Vector2.zero, _line.uvRect.size);
        }
    }
}