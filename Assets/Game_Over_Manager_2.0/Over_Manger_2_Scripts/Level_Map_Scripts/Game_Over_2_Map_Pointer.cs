using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Over_2_Map_Pointer : MonoBehaviour
{
    public float speed = 10f;

    public void Point_On_Level_Reached(Vector3 target)
    {
        StartCoroutine(Move_Pointer(target));
    }

    private IEnumerator Move_Pointer(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.0003)
        {
            var step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            yield return null;
        }
    }
}