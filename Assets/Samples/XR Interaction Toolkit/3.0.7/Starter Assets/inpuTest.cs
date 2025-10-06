using UnityEngine;
using UnityEngine.InputSystem;

public class InputTest : MonoBehaviour
{
    public InputActionReference grip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Subscribe to the grip action performed event
        grip.action.performed += Grip;
    }

    // Update is called once per frame
    void Update()
    {
        // No need to do anything here for the grip button press, as it is handled by the event
    }

    // This is the callback function for the grip action event
    private void Grip(InputAction.CallbackContext context)
    {
        // Get the positions of the objects
    Vector3 xrOriginPosition = GameObject.Find("XR Origin (XR Rig)").transform.position;
    Vector3 remotePosition = GameObject.Find("Remote").transform.position;

    // Calculate the distance between the XR Origin and Remote
    float distance = Vector3.Distance(xrOriginPosition, remotePosition);

    // Check if the distance is less than or equal to the threshold
    if (distance <= 1.0f) // You can adjust the threshold to whatever value you need
    {
        // Enable gravity on the TV when the distance condition is met
        GameObject.Find("TV_FlatWallMounted").GetComponent<Rigidbody>().useGravity = true;
    }
    }

    // Unsubscribe from the event when the object is disabled to avoid memory leaks
    void OnDisable()
    {
        grip.action.performed -= Grip;
    }
}
