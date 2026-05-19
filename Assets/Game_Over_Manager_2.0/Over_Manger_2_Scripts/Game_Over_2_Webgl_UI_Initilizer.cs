using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Over_2_Webgl_UI_Initilizer : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ObjectsToDisable = new List<GameObject>();

    private void Start()
    {
        DisableForWebGL();
    }

    private void DisableForWebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        for (int i = 0; i < ObjectsToDisable.Count; i++)
        {
            GameObject obj = ObjectsToDisable[i];
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
#endif
    }
}
