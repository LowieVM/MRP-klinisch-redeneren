using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance => instance;

    private const string SfxVolumeKey = "SFXVolume";

    public float SfxVolume { get; private set; } = 0.5f; // Default 50%

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSfxVolume();
    }

    public void SetSfxVolume(float normalizedVolume)
    {
        SfxVolume = normalizedVolume;
        PlayerPrefs.SetFloat(SfxVolumeKey, normalizedVolume * 100f);
        PlayerPrefs.Save();
    }

    private void LoadSfxVolume()
    {
        float saved = PlayerPrefs.GetFloat(SfxVolumeKey, 50f);
        SfxVolume = saved / 100f;
    }
}
