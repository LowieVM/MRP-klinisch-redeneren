using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MenuController : MonoBehaviour
{
    [Tooltip("Fallback wait time when the clicked button has no AudioClip.")]
    [SerializeField] private float fallbackDelay = 0.15f;

    [Tooltip("Optional VideoPlayer that will play before the target scene loads.")]
    [SerializeField] private VideoPlayer introVideoPlayer;

    public Canvas canvas; 

    // Prevent double-starts
    private bool isLoading = false;

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the built application
        Application.Quit();
#endif
    }

    // Keep the original behavior (simple delay)
    public void LoadSceneAfterDelay(string sceneName)
    {
        StartCoroutine(LoadSceneAfterDelayCoroutine(sceneName));
    }

    private IEnumerator LoadSceneAfterDelayCoroutine(string sceneName)
    {
        yield return new WaitForSeconds(fallbackDelay);

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    // Call this from the Button's OnClick() to play the configured video (if any)
    // before loading the requested scene.
    public void LoadSceneAfterVideo(string sceneName)
    {
        StartCoroutine(LoadSceneAfterVideoCoroutine(sceneName));
    }

    private IEnumerator LoadSceneAfterVideoCoroutine(string sceneName)
    {
        if (isLoading)
            yield break;

        if (canvas != null)
        {
            canvas.enabled = false;
        }

        isLoading = true;

        // No scene name -> do nothing and reset flag
        if (string.IsNullOrEmpty(sceneName))
        {
            if (canvas != null)
            {
                canvas.enabled = true;
            }

            isLoading = false;
            yield break;
        }

        // If no VideoPlayer is assigned or it has no clip/url, fall back to the small delay
        if (introVideoPlayer == null || (introVideoPlayer.clip == null && string.IsNullOrEmpty(introVideoPlayer.url)))
        {
            yield return new WaitForSeconds(fallbackDelay);
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        bool finished = false;

        void OnVideoFinished(VideoPlayer vp)
        {
            finished = true;
        }

        introVideoPlayer.loopPointReached += OnVideoFinished;

        // Ensure video is prepared/started
        if (!introVideoPlayer.isPrepared)
        {
            introVideoPlayer.Prepare();
            float prepTimeout = 5f;
            while (!introVideoPlayer.isPrepared && prepTimeout > 0f)
            {
                prepTimeout -= Time.deltaTime;
                yield return null;
            }
            // If still not prepared after timeout, fall back
            if (!introVideoPlayer.isPrepared)
            {
                introVideoPlayer.loopPointReached -= OnVideoFinished;
                yield return new WaitForSeconds(fallbackDelay);
                SceneManager.LoadScene(sceneName);
                yield break;
            }
        }

        introVideoPlayer.Play();

        // Safety timeout in case something goes wrong with the event
        float safetyTimeout = Mathf.Max((float)introVideoPlayer.length + 2f, 10f);

        while (!finished && safetyTimeout > 0f)
        {
            safetyTimeout -= Time.deltaTime;
            yield return null;
        }

        introVideoPlayer.loopPointReached -= OnVideoFinished;

        // If safety timeout expired but video still playing, try to stop it
        if (safetyTimeout <= 0f && introVideoPlayer.isPlaying)
            introVideoPlayer.Stop();

        isLoading = false;
        SceneManager.LoadScene(sceneName);
    }
}
