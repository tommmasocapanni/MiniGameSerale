using UnityEngine;
using System.Collections;

public class UFOController : MonoBehaviour
{
    public float speed = 10f;
    public float rotationSpeed = 100f;
    private PlayerController playerController;
    private bool isPlayerInUFO = false;
    public Transform cameraTransform; // Reference to the camera transform
    public Vector3 cameraOffset = new Vector3(0, 5, -20); // Adjusted camera offset for a wider view

    void Update()
    {
        if (isPlayerInUFO)
        {
            // Input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Calculate movement direction
            Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;

            // Move the UFO
            if (movement.magnitude > 0)
            {
                Vector3 move = transform.right * horizontal + transform.forward * vertical;
                transform.position += move * speed * Time.deltaTime;
            }

            // Handle UFO rotation
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            // Rotate the UFO based on mouse input
            transform.Rotate(Vector3.up * mouseX);
            transform.Rotate(Vector3.left * mouseY);

            // Update camera position
            Vector3 desiredCameraPosition = transform.position + cameraOffset;
            cameraTransform.position = desiredCameraPosition;
            cameraTransform.LookAt(transform.position);

            // Ascend and descend
            if (Input.GetKey(KeyCode.Space))
            {
                transform.position += Vector3.up * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.Z))
            {
                transform.position += Vector3.down * speed * Time.deltaTime;
            }

            // Exit UFO
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(ExitUFO());
            }
        }
    }

    public void EnterUFO(PlayerController player)
    {
        isPlayerInUFO = true;
        playerController = player;
        player.animator.SetBool("isDriving", true); // Ensure the driving animation is set
    }

    public IEnumerator ExitUFO()
    {
        // Rotate the UFO glass to open
        Quaternion targetRotation = transform.Find("glass").localRotation * Quaternion.Euler(0, 0, -80);
        float rotationDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            transform.Find("glass").localRotation = Quaternion.Slerp(transform.Find("glass").localRotation, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.Find("glass").localRotation = targetRotation; // Ensure the final rotation is exact

        // Eject the player from the UFO
        playerController.transform.SetParent(null);
        playerController.rb.isKinematic = false;
        playerController.rb.AddForce(transform.forward * 5f, ForceMode.Impulse);
        playerController.enabled = true; // Enable player controls
        playerController.animator.SetBool("isDriving", false); // Stop the driving animation
        isPlayerInUFO = false;
        playerController = null;
    }
}
