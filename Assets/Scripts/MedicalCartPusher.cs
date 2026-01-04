using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class MedicalCartPusher : MonoBehaviour
{
    [Header("Cart Settings")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float drag = 2f;
    [SerializeField] private float angularDrag = 5f;

    [Header("Handle References")]
    [SerializeField] private Transform leftHandle;
    [SerializeField] private Transform rightHandle;

    [Header("Wheel Settings")]
    [SerializeField] private bool lockWheelRotation = false;
    [SerializeField] private float wheelTurnSpeed = 50f;

    private Rigidbody rb;
    private XRGrabInteractable leftGrabInteractable;
    private XRGrabInteractable rightGrabInteractable;

    private bool leftHandGrabbing = false;
    private bool rightHandGrabbing = false;

    private Vector3 leftHandPrevPos;
    private Vector3 rightHandPrevPos;

    private Transform leftController;
    private Transform rightController;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Configure rigidbody for realistic cart physics
        rb.mass = 15f;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Freeze rotation on X and Z to prevent tipping
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        SetupHandles();
    }

    void SetupHandles()
    {
        // Setup left handle
        if (leftHandle != null)
        {
            leftGrabInteractable = leftHandle.gameObject.GetComponent<XRGrabInteractable>();
            if (leftGrabInteractable == null)
            {
                leftGrabInteractable = leftHandle.gameObject.AddComponent<XRGrabInteractable>();
            }

            leftGrabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            leftGrabInteractable.throwOnDetach = false;

            leftGrabInteractable.selectEntered.AddListener(OnLeftHandGrab);
            leftGrabInteractable.selectExited.AddListener(OnLeftHandRelease);
        }

        // Setup right handle
        if (rightHandle != null)
        {
            rightGrabInteractable = rightHandle.gameObject.GetComponent<XRGrabInteractable>();
            if (rightGrabInteractable == null)
            {
                rightGrabInteractable = rightHandle.gameObject.AddComponent<XRGrabInteractable>();
            }

            rightGrabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            rightGrabInteractable.throwOnDetach = false;

            rightGrabInteractable.selectEntered.AddListener(OnRightHandGrab);
            rightGrabInteractable.selectExited.AddListener(OnRightHandRelease);
        }
    }

    void OnLeftHandGrab(SelectEnterEventArgs args)
    {
        leftHandGrabbing = true;
        leftController = args.interactorObject.transform;
        leftHandPrevPos = leftController.position;
    }

    void OnLeftHandRelease(SelectExitEventArgs args)
    {
        leftHandGrabbing = false;
        leftController = null;
    }

    void OnRightHandGrab(SelectEnterEventArgs args)
    {
        rightHandGrabbing = true;
        rightController = args.interactorObject.transform;
        rightHandPrevPos = rightController.position;
    }

    void OnRightHandRelease(SelectExitEventArgs args)
    {
        rightHandGrabbing = false;
        rightController = null;
    }

    void FixedUpdate()
    {
        if (!leftHandGrabbing && !rightHandGrabbing)
            return;

        Vector3 pushDirection = Vector3.zero;
        int activeHands = 0;

        // Calculate push from left hand
        if (leftHandGrabbing && leftController != null)
        {
            Vector3 handDelta = leftController.position - leftHandPrevPos;
            pushDirection += handDelta;
            leftHandPrevPos = leftController.position;
            activeHands++;
        }

        // Calculate push from right hand
        if (rightHandGrabbing && rightController != null)
        {
            Vector3 handDelta = rightController.position - rightHandPrevPos;
            pushDirection += handDelta;
            rightHandPrevPos = rightController.position;
            activeHands++;
        }

        if (activeHands > 0)
        {
            // Average the push direction
            pushDirection /= activeHands;

            // Project push onto horizontal plane
            pushDirection.y = 0;

            if (pushDirection.magnitude > 0.001f)
            {
                // Apply force based on push direction
                Vector3 force = pushDirection * (pushForce / Time.fixedDeltaTime);
                rb.AddForce(force, ForceMode.Force);

                // Limit max speed
                if (rb.linearVelocity.magnitude > maxSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
                }

                // Rotate cart to face movement direction when both hands are used
                if (leftHandGrabbing && rightHandGrabbing && !lockWheelRotation)
                {
                    Vector3 moveDir = rb.linearVelocity;
                    moveDir.y = 0;

                    if (moveDir.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                        Quaternion newRotation = Quaternion.RotateTowards(
                            transform.rotation,
                            targetRotation,
                            wheelTurnSpeed * Time.fixedDeltaTime
                        );

                        rb.MoveRotation(newRotation);
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (leftGrabInteractable != null)
        {
            leftGrabInteractable.selectEntered.RemoveListener(OnLeftHandGrab);
            leftGrabInteractable.selectExited.RemoveListener(OnLeftHandRelease);
        }

        if (rightGrabInteractable != null)
        {
            rightGrabInteractable.selectEntered.RemoveListener(OnRightHandGrab);
            rightGrabInteractable.selectExited.RemoveListener(OnRightHandRelease);
        }
    }
}