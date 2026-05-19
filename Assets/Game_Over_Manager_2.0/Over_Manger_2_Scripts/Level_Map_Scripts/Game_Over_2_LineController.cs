using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Over_2_LineController : MonoBehaviour
{
    [SerializeField] private LineRenderer lr;
    private List<Transform> points = new List<Transform>();

    public void SetUpLine(List<Transform> points)
    {
        lr.positionCount = points.Count;
        this.points = points;
    }

    private void Update()
    {
        for (int i = 0; i < points.Count; i++)
        {
            lr.SetPosition(i, points[i].position);
        }
    }
}