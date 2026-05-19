using System.Collections;
using System.Collections.Generic;
//using horizon.Logging;
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
        //Play_Sfx("Bubble_Sfx");
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
        //if (PlayerPrefs.GetInt(Constants.DLC_IS_OPEN_FROM_MAP, 0) == 0)
        //{
        //    AnalyticsTracker.Instance.TrackScreenVisit(Constants.SCENE_GAMES);
        //    LoadScene(Constants.SCENE_GAMES);
        //}
        //else
        //{
        //    AnalyticsTracker.Instance.TrackScreenVisit(Constants.SCENE_MAP_SCENE);
        //    LoadScene(Constants.SCENE_MAP_SCENE);
        //}
        Destroy(Game_Over_2_AudioManager.audioManInstance.gameObject);
        Destroy(Game_Over_2_Manager.instance.gameObject);
    }

    private IEnumerator Load_Scene(string sceneName_Path)
    {
        yield return new WaitForSeconds(.6f);
        //Timer
        _sliderLoading.SetActive(true);
        if (_sliderLoading.activeInHierarchy)
        {
            //Begin to load the Scene you specify
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName_Path);
            //Don't let the Scene activate until you allow it to
            asyncOperation.allowSceneActivation = false;
            //When the load is still in progress, output the Text and progress bar

            while (!asyncOperation.isDone)
            {
                //Output the current progress
                _fill.value = Mathf.Lerp(_fill.value, asyncOperation.progress, 2 * Time.deltaTime);

                //Check if the load has finished
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