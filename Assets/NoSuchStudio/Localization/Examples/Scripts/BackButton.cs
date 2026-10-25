using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class BackButton : MonoBehaviour
{
    public void OnClick()
    {
        
        
        
        
        
        {
            SceneManager.LoadScene(0);
        }
    }
}
