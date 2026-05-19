using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

#if UNITY_WEBGL && !UNITY_EDITOR

using FirebaseWebGL.Scripts.FirebaseBridge;
using FirebaseWebGL.Examples.Utils;
using FirebaseWebGL.Scripts.Objects; 

#elif !UNITY_EDITOR

//using Firebase.Database;
//using Firebase.Extensions;

#endif

public class Game_Over_2_FB_Interactions : MonoBehaviour
{
    private static GameObject _myGOInstance;
    private static bool isWebGLTransactionDone = false;
    private static object result = null;

    private static GameObject Get_Instance()
    {
        if (_myGOInstance == null)
        {
            GameObject gm = new GameObject();
            _myGOInstance = Instantiate(gm);
            _myGOInstance.AddComponent<Game_Over_2_FB_Interactions>();
        }
        return _myGOInstance;
    }

    public static void Set_Item(string path, object value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        isWebGLTransactionDone = false;
        FirebaseDatabase.PostJSON(path, value.ToString(), Get_Instance().name, "Success_Transaction", "Success_Transaction");
#elif !UNITY_EDITOR
        //FirebaseDatabase.DefaultInstance.GetReference(path).SetValueAsync(value);
#endif
    }

    public static IEnumerator Update_Dictionnary(string path, Dictionary<string, object> dictionary)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        isWebGLTransactionDone = false;
        FirebaseDatabase.UpdateJSON(path, JsonConvert.SerializeObject(dictionary), Get_Instance().name, "Success_Transaction", "Success_Transaction");
        while (!isWebGLTransactionDone)
        {
            yield return null;
        }
#elif !UNITY_EDITOR
        //bool isDoneFetching = false;
        //FirebaseDatabase.DefaultInstance.RootReference.UpdateChildrenAsync(dictionary).ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsFaulted)
        //    {
        //        isDoneFetching = true;
        //    }
        //    else if (task.IsCompleted)
        //    {
        //        isDoneFetching = true;
        //    }
        //});
        //while (!isDoneFetching)
        //{
        //    yield return null;
        //}
        yield return result;
#else
        yield return null;
#endif
    }

    public static IEnumerator Get_Value_WEC(string path)
    {
        result = null;
#if UNITY_WEBGL && !UNITY_EDITOR
        isWebGLTransactionDone = false;
        FirebaseDatabase.GetJSON(path, Get_Instance().name, "FetchData", "ErrorObject");
        while (!isWebGLTransactionDone)
        {
            yield return null;
        }
        yield return result;

#else 
        yield return null;
#endif
    }

    public static IEnumerator Get_Data(string path)
    {

        result = null;

#if UNITY_WEBGL && !UNITY_EDITOR
        isWebGLTransactionDone = false;
        FirebaseDatabase.GetJSON(path, Get_Instance().name, "FetchData", "ErrorObject");
        while (!isWebGLTransactionDone)
        {
            yield return null;
        }

        if (result != null && result.ToString() != "null" && !string.IsNullOrEmpty(result.ToString()))
        {
            result = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.ToString());
            yield return result;
        }
        else
        {
            yield return result;
        }
#else
        yield return null;
#endif
    }

    public void FetchData(string data)
    {
        result = data;
        isWebGLTransactionDone = true;
    }

    public void Success_Transaction(string data)
    {
        isWebGLTransactionDone = true;
    }

    public void ErrorObject(string error)
    {
        result = null;
        isWebGLTransactionDone = true;
    }
}