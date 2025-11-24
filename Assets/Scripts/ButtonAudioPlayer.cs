using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class ButtonAudioPlayer : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning($"{nameof(ButtonAudioPlayer)} on '{gameObject.name}' has no AudioSource.");
            return;
        }

        audioSource.spatialBlend = 0f; // UI sound = 2D
        audioSource.playOnAwake = false;
    }

    // Plays immediately on pointer down (earlier than OnClick), so there's no perceptible delay.
    public void OnPointerDown(PointerEventData eventData)
    {
        Play();
    }

    // Also useful to hook in the inspector (Button -> OnClick)
    public void Play()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        float volumeScale = volume;
        // If you use a SoundManager with a global SFX slider (as in your project), apply it:
        if (SoundManager.Instance != null)
            volumeScale *= SoundManager.Instance.SfxVolume;

        audioSource.PlayOneShot(audioSource.clip, volumeScale);
    }
}
