using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class FootstepSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f; // Seconds between steps
    public float minSpeed = 0.1f; // Minimum speed before footsteps play (meters/second)
    public Vector2 pitchVariation = new Vector2(0.9f, 1.1f);

    [Header("Movement detection (XR-friendly)")]
    [Tooltip("If true the script measures transform.position delta instead of CharacterController.velocity")]
    public bool usePositionDelta = true;

    private ContinuousMoveProvider moveProvider;
    private CharacterController characterController;
    private float stepTimer;
    private Vector3 lastPosition;

    private void Start()
    {
        moveProvider = GetComponent<ContinuousMoveProvider>();
        // CharacterController may sit on a parent (XR Origin)
        characterController = GetComponent<CharacterController>() ?? GetComponentInParent<CharacterController>();

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

        if (footstepClips == null || footstepClips.Length == 0)
        {
            Debug.LogWarning("[FootstepSound] No footstep clips assigned. Assign AudioClips in the inspector.");
        }

        lastPosition = transform.position;
    }

    private void Update()
    {
        float speed = 0f;

        if (usePositionDelta)
        {
            // Works for XR rigs that move the transform directly
            Vector3 delta = transform.position - lastPosition;
            speed = delta.magnitude / Mathf.Max(Time.deltaTime, 1e-6f);
            lastPosition = transform.position;
        }
        else
        {
            if (characterController != null)
                speed = characterController.velocity.magnitude;
            else
                speed = 0f;
        }

        if (speed > minSpeed)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips != null && footstepClips.Length > 0)
        {
            var clip = footstepClips[Random.Range(0, footstepClips.Length)];
            footstepSource.pitch = Random.Range(pitchVariation.x, pitchVariation.y);
            // Use PlayOneShot so we don't reassign the AudioSource.clip or cut other sounds
            footstepSource.PlayOneShot(clip);
        }
    }
}