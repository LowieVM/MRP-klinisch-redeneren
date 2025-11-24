using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Tooltip("Fallback wait time when the clicked button has no AudioClip.")]
    [SerializeField] private float fallbackDelay = 0.15f;

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the built application
        Application.Quit();
#endif
    }

    // Call this from the Button's OnClick() (drag the MenuController object and
    // choose LoadSceneAfterDelay(String) and type the scene name).
    // This method only delays the load — it does NOT try to locate or play the sound.
    // Play the button sound immediately from the button itself (see ButtonAudioPlayer).
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
}
