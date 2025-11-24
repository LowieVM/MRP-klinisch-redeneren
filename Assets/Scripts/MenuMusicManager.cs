using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuMusicManager : MonoBehaviour
{
    private static MenuMusicManager instance;
    public static MenuMusicManager Instance => instance;

    private AudioSource audioSource;

    public string stopMusicOnScene = "FullHospitalScene";
    public float fadeOutDuration = 1.5f;

    private bool isFadingOut = false;

    private const string VolumeKey = "MenuVolume";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        audioSource = GetComponent<AudioSource>();

        DontDestroyOnLoad(gameObject);

        // Load saved volume (default 50 → 0.5 in AudioSource)
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 50f) / 100f;
        audioSource.volume = savedVolume;

        if (!audioSource.isPlaying)
            audioSource.Play();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SetVolume(float normalizedVolume)
    {
        if (!isFadingOut)
            audioSource.volume = normalizedVolume;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == stopMusicOnScene && !isFadingOut)
        {
            StartCoroutine(FadeOutAndStop());
        }
    }

    private IEnumerator FadeOutAndStop()
    {
        isFadingOut = true;

        float startVolume = audioSource.volume;
        float t = 0f;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
            yield return null;
        }

        audioSource.Stop();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
