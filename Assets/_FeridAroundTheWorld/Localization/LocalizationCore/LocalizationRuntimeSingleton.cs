using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.horizon.LocalizationSystem
{
    
    
    public class LocalizationRuntimeSingleton : MonoBehaviour
    {
        private static LocalizationRuntimeSingleton instance;


        private void Awake()
        {
          
            InitSingleton();
        }



        #region --- Singleton
        public static LocalizationRuntimeSingleton GetInstance()
        {
            if (instance == null)
            {
                Debug.LogError(nameof(LocalizationRuntimeSingleton) + " instance is null");
            }
            return instance;
        }
        private void InitSingleton()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

    }
}