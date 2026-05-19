using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Over_2_CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public Game_Over_2_CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}
