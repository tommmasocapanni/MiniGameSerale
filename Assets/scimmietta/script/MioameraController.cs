using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The target the camera will follow (the player)
    public Vector3 offset; // Offset position of the camera relative to the target
    public float followSpeed = 10f; // Speed at which the camera follows the target
    public float rotationSpeed = 5f; // Speed at which the camera rotates to follow the target

    void LateUpdate()
    {
        // Calculate the desired position
        Vector3 desiredPosition = target.position + offset;
        // Smoothly interpolate to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Smoothly rotate the camera to face the target
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}