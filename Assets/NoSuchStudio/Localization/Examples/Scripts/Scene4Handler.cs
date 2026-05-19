using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using NoSuchStudio.Variables;

public class Scene4Handler : MonoBehaviour
{

    [SerializeField] VariablesSource _source;

    public void OnScoreChange(float score) {
        _source.SetVariable("score", score.ToString());
    }

    public void OnUserChange(string username) {
        _source.SetVariable("user", username);
    }
}
