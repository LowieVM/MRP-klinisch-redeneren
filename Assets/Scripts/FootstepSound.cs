using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class FootstepSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource footstepSource;
    [Tooltip("If true the script will use the AudioSource.clip. If the AudioSource.clip is set to loop it will play continuously while moving and stop when movement stops. If not looping the clip will be played once per step.")]
    public bool useAudioSourceClip = true;
    public float stepInterval = 0.2f;
    public float minSpeed = 1f;
    public Vector2 pitchVariation = new Vector2(0.9f, 1.1f);

    [Header("Movement detection (XR-friendly)")]
    [Tooltip("If true the script measures transform.position delta instead of CharacterController.velocity")]
    public bool usePositionDelta = true;

    [Tooltip("Transform used to detect movement. If null the script chooses a sensible default (CharacterController, Camera.main, or this.transform).")]
    public Transform movementSource;

    [Header("Debug")]
    public bool debugLogging = false;

    private ContinuousMoveProvider moveProvider;
    private CharacterController characterController;
    private float stepTimer;
    private Vector3 lastPosition;
    private bool _wasPlayingLoop = false;

    private void Start()
    {
        moveProvider = GetComponent<ContinuousMoveProvider>();

        // If movementSource not set, attempt to find best candidate
        if (movementSource == null)
        {
            characterController = GetComponent<CharacterController>() ?? GetComponentInParent<CharacterController>();
            if (characterController != null)
            {
                movementSource = characterController.transform;
                if (debugLogging) Debug.Log("[FootstepSound] Using CharacterController.transform for movement detection.");
            }
            else if (Camera.main != null)
            {
                movementSource = Camera.main.transform;
                if (debugLogging) Debug.Log("[FootstepSound] Using Camera.main.transform for movement detection.");
            }
            else
            {
                movementSource = this.transform;
                if (debugLogging) Debug.Log("[FootstepSound] Using this.transform for movement detection.");
            }
        }
        else
        {
            if (characterController == null)
                characterController = movementSource.GetComponent<CharacterController>() ?? movementSource.GetComponentInParent<CharacterController>();
        }

        if (footstepSource == null)
        {
            footstepSource = GetComponent<AudioSource>() ?? GetComponentInChildren<AudioSource>();
        }

        if (footstepSource == null)
        {
            Debug.LogError("[FootstepSound] No AudioSource assigned or found on GameObject/children.");
            enabled = false;
            return;
        }

        if (useAudioSourceClip && footstepSource.clip == null)
        {
            Debug.LogWarning("[FootstepSound] useAudioSourceClip is true but AudioSource.clip is null. Assign a clip in the AudioSource or disable useAudioSourceClip.");
        }

        lastPosition = movementSource.position;

        if (footstepSource.spatialBlend < 0.9f && debugLogging)
            Debug.Log("[FootstepSound] Consider setting AudioSource.spatialBlend to 1 for 3D footsteps.");
    }

    private void Update()
    {
        float speed = 0f;

        if (usePositionDelta)
        {
            Vector3 delta = movementSource.position - lastPosition;
            speed = delta.magnitude / Mathf.Max(Time.deltaTime, 1e-6f);
            lastPosition = movementSource.position;
        }
        else
        {
            if (characterController != null)
                speed = characterController.velocity.magnitude;
            else
                speed = 0f;
        }

        if (debugLogging && speed > 0.01f)
            Debug.Log($"[FootstepSound] speed={speed:F2}, stepTimer={stepTimer:F2}");

        if (useAudioSourceClip && footstepSource.clip != null)
        {
            // If the AudioSource.clip is set to loop, use continuous play while moving and stop when stopped.
            if (footstepSource.loop)
            {
                if (speed > minSpeed)
                {
                    if (!footstepSource.isPlaying)
                    {
                        footstepSource.Play();
                        if (debugLogging) Debug.Log("[FootstepSound] Started looped footstep AudioSource.");
                    }
                    _wasPlayingLoop = true;
                }
                else
                {
                    if (_wasPlayingLoop && footstepSource.isPlaying)
                    {
                        footstepSource.Stop();
                        if (debugLogging) Debug.Log("[FootstepSound] Stopped looped footstep AudioSource.");
                    }
                    _wasPlayingLoop = false;
                }

                // No per-step timing when using a looping clip.
                stepTimer = 0f;
            }
            else
            {
                // Non-looping clip in AudioSource used as a per-step sound -> PlayOneShot each step
                HandlePerStepPlayback(speed, footstepSource.clip);
            }
        }
        else
        {
            // No AudioSource.clip usage: still play the AudioSource.clip if present - but warn
            if (footstepSource.clip != null)
                HandlePerStepPlayback(speed, footstepSource.clip);
            else
            {
                // Nothing to play
                stepTimer = 0f;
            }
        }
    }

    private void HandlePerStepPlayback(float speed, AudioClip clip)
    {
        if (speed > minSpeed)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                footstepSource.pitch = Random.Range(pitchVariation.x, pitchVariation.y);
                footstepSource.PlayOneShot(clip);
                if (debugLogging) Debug.Log("[FootstepSound] Played per-step footstep clip.");
                stepTimer = 0f;
            }
        }
        else
        {
            // stop any currently-playing looping AudioSource if it somehow was left playing
            if (footstepSource.isPlaying && footstepSource.loop)
            {
                footstepSource.Stop();
                if (debugLogging) Debug.Log("[FootstepSound] Stopped looping AudioSource because speed is below threshold.");
            }
            stepTimer = 0f;
        }
    }
}
