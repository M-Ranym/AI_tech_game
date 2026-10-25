using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_Over_2_LoadScene : MonoBehaviour
{
    private Game_Over_2_AudioManager _audioManager;

    [SerializeField] private GameObject _LoadingContainer;
    [SerializeField] private GameObject _loadingPanel;

    [SerializeField] private GameObject _sliderLoading;
    [SerializeField] private Slider _fill;

    private bool isPressed = false;

    [SerializeField] private AnimType _animType;

    public void LoadScene(string path)
    {
        if (!isPressed)
        {
            isPressed = true;
            Time.timeScale = 1;
            Show_Loading_Panel();
            StartCoroutine(Load_Scene(path));
        }
    }

    private void Show_Loading_Panel()
    {
        
        _LoadingContainer.SetActive(true);
        switch (_animType)
        {
            case AnimType.DROP_DOWN:
                Drop_Down_Anim();
                break;

            case AnimType.SLIDE_UPWARDS:
                Slide_Upwards_Anim();
                break;

            case AnimType.SCALE_DOWN:
                Scale_Logo_Anim();
                break;
        }
    }

    private void Slide_Upwards_Anim()
    {
        _LoadingContainer.LeanMoveLocalY(380, .75f).setEaseOutBack();
    }

    private void Scale_Logo_Anim()
    {
        _LoadingContainer.LeanScale(Vector2.one * 0.55f, 0.3f).setEaseInOutExpo();
        _loadingPanel.LeanScale(Vector2.one * 2, .3f).setEaseInCirc().delay = .3f;
    }

    private void Drop_Down_Anim()
    {
        _LoadingContainer.LeanMoveLocalY(0, .5f).setEaseOutBack();
        _loadingPanel.LeanScale(Vector2.one, .3f).setEaseInCirc().delay = .3f;
    }

    public void ExitBtn()
    {
        Drop_Down_Anim();
        
        
        
        
        
        
        
        
        
        
        Destroy(Game_Over_2_AudioManager.audioManInstance.gameObject);
        Destroy(Game_Over_2_Manager.instance.gameObject);
    }

    private IEnumerator Load_Scene(string sceneName_Path)
    {
        yield return new WaitForSeconds(.6f);
        
        _sliderLoading.SetActive(true);
        if (_sliderLoading.activeInHierarchy)
        {
            
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName_Path);
            
            asyncOperation.allowSceneActivation = false;
            

            while (!asyncOperation.isDone)
            {
                
                _fill.value = Mathf.Lerp(_fill.value, asyncOperation.progress, 2 * Time.deltaTime);

                
                if (asyncOperation.progress >= 0.9f)
                {
                    _fill.value = 1f;
                    asyncOperation.allowSceneActivation = true;
                }
                yield return null;
            }
        }
    }
}

public enum AnimType
{
    NONE,
    DROP_DOWN,
    SLIDE_UPWARDS,
    SCALE_DOWN
}