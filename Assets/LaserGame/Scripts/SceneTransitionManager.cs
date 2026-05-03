using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    public Canvas fadeCanvas;
    public Image fadeImage;
    public float fadeDuration = 0.4f;

    private bool _isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }
    }

    public void LoadScene(string sceneName)
    {
        if (_isTransitioning) return;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        _isTransitioning = true;
        fadeImage.raycastTarget = true;

        yield return fadeImage.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.05f);

        yield return fadeImage.DOFade(0f, fadeDuration).SetUpdate(true).WaitForCompletion();

        fadeImage.raycastTarget = false;
        _isTransitioning = false;
    }
}
