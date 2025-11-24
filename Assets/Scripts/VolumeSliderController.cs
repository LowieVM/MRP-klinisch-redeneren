using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeSliderController : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_Text valueText;

    [Tooltip("Set to 'MenuVolume' for music, 'SFXVolume' for sound effects")]
    public string VolumeKey;

    private void Start()
    {
        if (volumeSlider == null)
        {
            Debug.LogError("VolumeSliderController: Slider not assigned.");
            return;
        }

        if (string.IsNullOrEmpty(VolumeKey))
        {
            Debug.LogError("VolumeSliderController: VolumeKey not assigned.");
            return;
        }

        // Slider bounds 0–100
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 100f;

        // Load saved volume (50 default)
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 50f);
        volumeSlider.value = savedVolume;

        // Update text
        UpdateValueText(savedVolume);

        // Apply volume depending on the key
        ApplyVolume(savedVolume);

        // Listen for slider changes
        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        UpdateValueText(value);

        // Save new volume value
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();

        // Apply volume depending on the key
        ApplyVolume(value);
    }

    private void ApplyVolume(float value)
    {
        float normalized = value / 100f;

        if (VolumeKey == "MenuVolume")
        {
            if (MenuMusicManager.Instance != null)
                MenuMusicManager.Instance.SetVolume(normalized);
        }
        else if (VolumeKey == "SFXVolume")
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.SetSfxVolume(normalized);
        }
        else
        {
            Debug.LogWarning($"VolumeSliderController: Unknown VolumeKey '{VolumeKey}'");
        }
    }

    private void UpdateValueText(float value)
    {
        if (valueText != null)
            valueText.text = Mathf.RoundToInt(value).ToString();
    }
}
