using UnityEngine;

public class VRDoor : MonoBehaviour
{
    [Header("References")]
    public Transform handle;       // The handle object (optional)
    public Transform door;         // The door object (pivoted at hinges)

    [Header("Settings")]
    public float doorOpenAngle = 90;    // How far the door swings open
    public float openSpeed = 2f;         // How quickly the door moves

    private bool isOpen = false;         // Track if the door is open
    private Quaternion doorClosedRot;    // Original rotation
    private Quaternion doorOpenRot;      // Target open rotation
    private bool isMoving = false;       // If door is currently moving

    void Start()
    {
        // Store starting (closed) rotation
        doorClosedRot = door.localRotation;

        // Calculate target open rotation
        doorOpenRot = door.localRotation * Quaternion.Euler(0, doorOpenAngle, 0);
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
        }
    }
}
