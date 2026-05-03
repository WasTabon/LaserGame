using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneBackBridge : MonoBehaviour
{
    public Button backButton;
    public string targetSceneName = "MainMenu";

    private void OnEnable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBack);
            backButton.onClick.AddListener(OnBack);
        }
    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(OnBack);
    }

    private void OnBack()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(targetSceneName);
        else
            SceneManager.LoadScene(targetSceneName);
    }
}
