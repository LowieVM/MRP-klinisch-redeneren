using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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

    // Use this from the Button's OnClick() (drag the MenuController object and select this method,
    // then type the scene name string in the inspector).
    // It finds the currently clicked button (EventSystem.current.currentSelectedGameObject),
    // plays its AudioSource (if present) and then loads the requested scene after the clip (or a short fallback).
    public void PlaySoundAndLoad(string sceneName)
    {
        StartCoroutine(PlaySoundAndLoadCoroutine(sceneName));
    }

    private IEnumerator PlaySoundAndLoadCoroutine(string sceneName)
    {
        float delay = fallbackDelay;

        if (EventSystem.current == null)
        {
            Debug.LogWarning($"{nameof(MenuController)}: No EventSystem found in the scene.");
        }
        else
        {
            var go = EventSystem.current.currentSelectedGameObject;
            if (go == null)
            {
                Debug.LogWarning($"{nameof(MenuController)}: No currently selected GameObject (button).");
            }
            else
            {
                var source = go.GetComponent<AudioSource>() ?? go.GetComponentInChildren<AudioSource>();
                if (source != null)
                {
                    // Ensure UI sounds are 2D and not play on awake
                    source.spatialBlend = 0f;
                    source.playOnAwake = false;

                    if (source.clip != null)
                    {
                        source.PlayOneShot(source.clip, source.volume);
                        float pitch = Mathf.Approximately(source.pitch, 0f) ? 1f : Mathf.Abs(source.pitch);
                        delay = source.clip.length / pitch;
                    }
                    else
                    {
                        Debug.LogWarning($"{nameof(MenuController)}: AudioSource on '{go.name}' has no AudioClip.");
                    }
                }
                else
                {
                    Debug.LogWarning($"{nameof(MenuController)}: No AudioSource found on '{go.name}' or its children.");
                }
            }
        }

        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}
