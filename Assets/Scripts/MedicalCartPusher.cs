using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class MedicalCartPusher : MonoBehaviour
{
    [Header("Cart Settings")]
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float drag = 3f;
    [SerializeField] private float dragWhenNotGrabbed = 10f;

    [Header("Handle References")]
    [SerializeField] private Transform leftHandle;
    [SerializeField] private Transform rightHandle;

    [Header("Hand Attachment")]
    [SerializeField] private bool useHandAttachment = true;
    [SerializeField] private float handAttachDistance = 0.15f;

    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable leftSimpleInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable rightSimpleInteractable;

    private bool leftHandGrabbing = false;
    private bool rightHandGrabbing = false;
    private Transform leftHandTransform;
    private Transform rightHandTransform;
    private ConfigurableJoint leftJoint;
    private ConfigurableJoint rightJoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Configure rigidbody
        rb.mass = 15f;
        rb.linearDamping = dragWhenNotGrabbed;
        rb.angularDamping = 5f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Freeze rotation to prevent tipping
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        SetupHandles();
    }

    void SetupHandles()
    {
        // Setup left handle with XRSimpleInteractable (NO MOVEMENT!)
        if (leftHandle != null)
        {
            // Remove any XRGrabInteractable if it exists
            var oldGrab = leftHandle.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (oldGrab != null)
            {
                if (Application.isPlaying)
                    Destroy(oldGrab);
                else
                    DestroyImmediate(oldGrab);
            }

            leftSimpleInteractable = leftHandle.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (leftSimpleInteractable == null)
            {
                leftSimpleInteractable = leftHandle.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            }

            // Make sure handle has collider
            if (leftHandle.GetComponent<Collider>() == null)
            {
                BoxCollider col = leftHandle.gameObject.AddComponent<BoxCollider>();
                col.size = new Vector3(0.15f, 0.2f, 0.15f);
            }

            leftSimpleInteractable.selectEntered.AddListener(OnLeftHandleGrabbed);
            leftSimpleInteractable.selectExited.AddListener(OnLeftHandleReleased);
        }

        // Setup right handle with XRSimpleInteractable (NO MOVEMENT!)
        if (rightHandle != null)
        {
            // Remove any XRGrabInteractable if it exists
            var oldGrab = rightHandle.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (oldGrab != null)
            {
                if (Application.isPlaying)
                    Destroy(oldGrab);
                else
                    DestroyImmediate(oldGrab);
            }

            rightSimpleInteractable = rightHandle.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (rightSimpleInteractable == null)
            {
                rightSimpleInteractable = rightHandle.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            }

            // Make sure handle has collider
            if (rightHandle.GetComponent<Collider>() == null)
            {
                BoxCollider col = rightHandle.gameObject.AddComponent<BoxCollider>();
                col.size = new Vector3(0.15f, 0.2f, 0.15f);
            }

            rightSimpleInteractable.selectEntered.AddListener(OnRightHandleGrabbed);
            rightSimpleInteractable.selectExited.AddListener(OnRightHandleReleased);
        }
    }

    void OnLeftHandleGrabbed(SelectEnterEventArgs args)
    {
        leftHandGrabbing = true;
        leftHandTransform = args.interactorObject.transform;

        // Reduce drag when grabbed
        rb.linearDamping = drag;

        // Create physics connection between hand and cart
        if (useHandAttachment)
        {
            CreateHandJoint(ref leftJoint, leftHandTransform, leftHandle);
        }

        Debug.Log("Left handle grabbed!");
    }

    void OnLeftHandleReleased(SelectExitEventArgs args)
    {
        leftHandGrabbing = false;
        leftHandTransform = null;

        // Destroy joint
        if (leftJoint != null)
        {
            Destroy(leftJoint);
            leftJoint = null;
        }

        // If no hands grabbing, increase drag
        if (!rightHandGrabbing)
        {
            rb.linearDamping = dragWhenNotGrabbed;
        }

        Debug.Log("Left handle released!");
    }

    void OnRightHandleGrabbed(SelectEnterEventArgs args)
    {
        rightHandGrabbing = true;
        rightHandTransform = args.interactorObject.transform;

        // Reduce drag when grabbed
        rb.linearDamping = drag;

        // Create physics connection between hand and cart
        if (useHandAttachment)
        {
            CreateHandJoint(ref rightJoint, rightHandTransform, rightHandle);
        }

        Debug.Log("Right handle grabbed!");
    }

    void OnRightHandleReleased(SelectExitEventArgs args)
    {
        rightHandGrabbing = false;
        rightHandTransform = null;

        // Destroy joint
        if (rightJoint != null)
        {
            Destroy(rightJoint);
            rightJoint = null;
        }

        // If no hands grabbing, increase drag
        if (!leftHandGrabbing)
        {
            rb.linearDamping = dragWhenNotGrabbed;
        }

        Debug.Log("Right handle released!");
    }

    void CreateHandJoint(ref ConfigurableJoint joint, Transform handTransform, Transform handleTransform)
    {
        // Create a temporary rigidbody on the hand to connect to
        GameObject handAnchor = new GameObject("HandAnchor");
        handAnchor.transform.position = handTransform.position;
        handAnchor.transform.rotation = handTransform.rotation;
        handAnchor.transform.parent = handTransform;

        Rigidbody handRb = handAnchor.AddComponent<Rigidbody>();
        handRb.isKinematic = true;
        handRb.useGravity = false;

        // Create joint on cart that connects to hand
        joint = rb.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = handRb;
        joint.anchor = transform.InverseTransformPoint(handleTransform.position);
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = Vector3.zero;

        // Configure joint to allow pulling/pushing but with some spring
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = handAttachDistance;
        joint.linearLimit = limit;

        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        // Add spring to make it feel natural
        JointDrive drive = new JointDrive();
        drive.positionSpring = 1000f;
        drive.positionDamper = 50f;
        drive.maximumForce = 500f;
        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;
    }

    void FixedUpdate()
    {
        // Update joint positions to follow hands
        if (leftHandGrabbing && leftJoint != null && leftHandTransform != null)
        {
            leftJoint.connectedBody.transform.position = leftHandTransform.position;
            leftJoint.connectedBody.transform.rotation = leftHandTransform.rotation;
        }

        if (rightHandGrabbing && rightJoint != null && rightHandTransform != null)
        {
            rightJoint.connectedBody.transform.position = rightHandTransform.position;
            rightJoint.connectedBody.transform.rotation = rightHandTransform.rotation;
        }

        // Limit max speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void OnDestroy()
    {
        if (leftSimpleInteractable != null)
        {
            leftSimpleInteractable.selectEntered.RemoveListener(OnLeftHandleGrabbed);
            leftSimpleInteractable.selectExited.RemoveListener(OnLeftHandleReleased);
        }

        if (rightSimpleInteractable != null)
        {
            rightSimpleInteractable.selectEntered.RemoveListener(OnRightHandleGrabbed);
            rightSimpleInteractable.selectExited.RemoveListener(OnRightHandleReleased);
        }

        // Clean up joints
        if (leftJoint != null) Destroy(leftJoint);
        if (rightJoint != null) Destroy(rightJoint);
    }
}