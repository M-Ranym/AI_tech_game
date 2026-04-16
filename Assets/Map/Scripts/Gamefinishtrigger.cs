using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFinishTrigger : MonoBehaviour
{
    public string mapSceneName = "Map";

    bool waitingForTap = false;

    void Update()
    {
        if (!waitingForTap) return;

        bool tapped = Input.GetMouseButtonDown(0)
                   || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (tapped)
            SceneManager.LoadScene(mapSceneName);
    }

    public void FinishGame()
    {
        if (waitingForTap) return;

        waitingForTap = true;
    }
}