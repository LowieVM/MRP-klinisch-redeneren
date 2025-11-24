using UnityEngine;

public class VRDoor : MonoBehaviour
{
    [Header("References")]
    public Transform handle;       // The handle object (optional)
    public Transform door;         // The door object (pivoted at hinges)

    [Header("Settings")]
    public float doorOpenAngle = 90;    // How far the door swings open
    public float openSpeed = 2f;         // How quickly the door moves

    [Header("Audio")]
    [Tooltip("AudioSource used to play the door sound. The clip assigned to this AudioSource will be played when the door starts opening or closing.")]
    public AudioSource audioSource;

    private bool isOpen = false;         // Track if the door is open
    private Quaternion doorClosedRot;    // Original rotation
    private Quaternion doorOpenRot;      // Target open rotation
    private bool isMoving = false;       // If door is currently moving

    void Start()
    {
        if (door == null)
        {
            Debug.LogError("[VRDoor] Door Transform is not assigned.");
            enabled = false;
            return;
        }

        // Store starting (closed) rotation
        doorClosedRot = door.localRotation;

        // Calculate target open rotation
        doorOpenRot = door.localRotation * Quaternion.Euler(0, doorOpenAngle, 0);

        // Auto-find AudioSource if none assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>() ?? door.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.Log("[VRDoor] No AudioSource found. Assign one or add a clip to the AudioSource to play door sounds.");
            }
        }
    }

    void Update()
    {
        if (isMoving)
        {
            // Select target rotation based on open/close state
            Quaternion targetRot = isOpen ? doorOpenRot : doorClosedRot;

            // Smoothly rotate toward target
            door.localRotation = Quaternion.Slerp(door.localRotation, targetRot, Time.deltaTime * openSpeed);

            // Stop when close enough
            if (Quaternion.Angle(door.localRotation, targetRot) < 1f)
            {
                door.localRotation = targetRot; // snap to target
                isMoving = false;
                Debug.Log(isOpen ? "Door Opened!" : "Door Closed!");
            }
        }
    }

    public void ToggleDoor()
    {
        if (!isMoving)
        {
            isOpen = !isOpen;   // flip state
            isMoving = true;    // start movement

            // Play the clip when the door starts moving (opening or closing)
            PlayDoorSound();
        }
    }

    private void PlayDoorSound()
    {
        if (audioSource == null)
            return;

        var clip = audioSource.clip;
        if (clip == null)
            return;

        // Play the AudioSource's clip once without altering AudioSource.clip or other settings
        float volume = SoundManager.Instance != null ? SoundManager.Instance.SfxVolume : 1f;
        audioSource.PlayOneShot(audioSource.clip, volume);
    }
}
