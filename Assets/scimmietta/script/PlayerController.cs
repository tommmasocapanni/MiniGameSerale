using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.1f;
    public float mouseSensitivity = 100f;
    public Transform cameraTransform; // Reference to the camera transform
    public Vector3 cameraOffset; // Offset position of the camera relative to the player
    public Vector3 cameraRotationOffset; // Offset rotation of the camera relative to the player
    public float cameraHeightOffset = 1f; // Offset height of the camera relative to the player's back
    public Animator animator; // Make this field public
    public Rigidbody rb; // Make this field public
    private Vector3 movement;
    private bool isGrounded;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Jetpack jetpack;
    public bool nearCar = false; // Make this field public
    public GameObject car; // Make this field public
    private bool isInCar = false;
    private bool isExitingCar = false;

    public GameObject ufo; // Reference to the UFO
    public Transform ufoGlass; // Reference to the UFO glass
    public Transform ufoSeatTrigger; // Reference to the UFO seat trigger
    private bool nearUFO = false; // Indicates if the player is near the UFO
    private bool isInUFO = false; // Indicates if the player is in the UFO

    private float targetJetpackRotationX = 0f; // Target rotation for jetpack propulsion

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
    }

    void Update()
    {
        // Input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction
        movement = new Vector3(horizontal, 0, vertical).normalized;

        // Update animations
        if (movement.magnitude > 0)
        {
            animator.SetBool("isRunning", true);

            // Move the character
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            rb.MovePosition(rb.position + move * speed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        // Modify the ground check to not interfere with jetpack
        if (jetpack == null) // Only check ground state if no jetpack
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
            
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                animator.SetBool("isJumping", true);
                isGrounded = false;
            }
        }

        // Handle jetpack activation
        if (jetpack != null)
        {
            Vector3 forceDirection = Vector3.zero;
            float targetRotationX = 0f;
            float targetRotationZ = 0f;

            if (Input.GetKey(KeyCode.Space) && jetpack.HasFuel())
            {
                // Apply force in the direction the camera is facing
                forceDirection += cameraTransform.forward + Vector3.up;
                jetpack.ConsumeFuel(Time.deltaTime);
                rb.useGravity = false; // Disable gravity while using the jetpack
            }
            else
            {
                // Ensure gravity is applied when the space key is not pressed
                rb.useGravity = true;
            }

            // Handle WASD movement and rotation
            if (Input.GetKey(KeyCode.W))
            {
                // Move forward
                forceDirection += cameraTransform.forward;
                targetRotationX = 30f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                // Move backward
                forceDirection -= cameraTransform.forward;
                targetRotationX = -30f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                // Move left
                forceDirection -= cameraTransform.right;
                targetRotationZ = -30f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                // Move right
                forceDirection += cameraTransform.right;
                targetRotationZ = 30f;
            }

            if (forceDirection != Vector3.zero)
            {
                rb.AddForce(forceDirection.normalized * jetpack.jetpackForce, ForceMode.Acceleration);
            }

            // Smoothly interpolate the rotation
            float smoothRotationX = Mathf.LerpAngle(transform.rotation.eulerAngles.x, targetRotationX, Time.deltaTime * 5f);
            float smoothRotationZ = Mathf.LerpAngle(transform.rotation.eulerAngles.z, targetRotationZ, Time.deltaTime * 5f);
            transform.rotation = Quaternion.Euler(smoothRotationX, transform.rotation.eulerAngles.y, smoothRotationZ);

            if (Input.GetKeyDown(KeyCode.E))
            {
                RemoveJetpack();
            }
        }

        // Mouse input for camera rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the character based on mouse input
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp rotation to prevent going below ground or too high

        // Smooth rotation for jetpack
        float currentRotationX = Mathf.LerpAngle(transform.localEulerAngles.x, targetJetpackRotationX, Time.deltaTime * 5f);
        transform.localRotation = Quaternion.Euler(currentRotationX, yRotation, 0f);

        // Update camera position and rotation
        Quaternion cameraRotation = Quaternion.Euler(xRotation + cameraRotationOffset.x, yRotation + cameraRotationOffset.y, cameraRotationOffset.z);
        Vector3 desiredCameraPosition = transform.position + cameraRotation * cameraOffset;
        desiredCameraPosition.y = Mathf.Max(transform.position.y + cameraHeightOffset, desiredCameraPosition.y); // Prevent camera from going below the character
        cameraTransform.position = desiredCameraPosition;
        cameraTransform.LookAt(transform.position + Vector3.up * cameraHeightOffset); // Look at the center of the character's back

        // Check for interaction with car
        if (nearCar && Input.GetKeyDown(KeyCode.E) && !isInCar)  // Aggiunto check !isInCar
        {
            if (car != null)
            {
                Debug.Log("Attempting to enter car");
                StartCoroutine(EnterCarCoroutine());
            }
            else
            {
                Debug.LogError("Car reference is null");
            }
        }

        // Check for interaction with UFO
        if (nearUFO && !isInUFO)
        {
            Debug.Log("Near UFO");
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E key pressed near UFO");
                StartCoroutine(EnterUFO());
            }
        }

        // Check for interaction with UFO seat
        if (isInUFO && nearUFO && ufoSeatTrigger != null)
        {
            Debug.Log("Near UFO seat");
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E key pressed near UFO seat");
                StartCoroutine(SitInUFO());
            }
        }

        // Check for exiting the UFO
        if (isInUFO && Input.GetKeyDown(KeyCode.Q))
        {
            ExitUFO();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("UFO"))
        {
            isGrounded = true;
            if (jetpack == null) // Resetta animazione salto solo se non ha il jetpack
            {
                animator.SetBool("isJumping", false);
            }
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            // Reset animazione salto solo se non ha il jetpack
            if (jetpack == null)
            {
                animator.SetBool("isJumping", false);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("UFO"))
        {
            isGrounded = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            Debug.Log("Player entered car trigger");
            nearCar = true;
            car = other.gameObject;
        }

        if (other.CompareTag("UFO"))
        {
            nearUFO = true;
            ufo = other.gameObject;
            ufoGlass = ufo.transform.Find("glass"); // Ensure the glass is named "glass"
            ufoSeatTrigger = ufo.transform.Find("seatTrigger"); // Ensure the seat trigger is named "seatTrigger"
            Debug.Log("UFO detected");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            if (!isInCar)  // Solo se non siamo dentro la macchina
            {
                Debug.Log("Player exited car trigger");
                nearCar = false;
                car = null;
            }
        }

        if (other.CompareTag("UFO"))
        {
            nearUFO = false;
            ufo = null;
            ufoGlass = null;
            ufoSeatTrigger = null;
            Debug.Log("UFO out of range");
        }
    }

    private void EnterCar()
    {
        Debug.Log("Entering car");
        isInCar = true;
        animator.SetTrigger("enterCar"); // Trigger the enter car animation
        StartCoroutine(EnterCarCoroutine());
    }

    private IEnumerator EnterCarCoroutine()
    {
        if (car == null)
        {
            Debug.LogError("Car reference is null!");
            yield break;
        }

        CarController carController = car.GetComponent<CarController>();
        if (carController == null)
        {
            Debug.LogError("CarController component not found on car!");
            yield break;
        }

        if (!isInCar)
        {
            isInCar = true;
            Debug.Log("Starting car enter sequence");
            
            // Riattiva questa riga per disabilitare il controller durante la guida
            enabled = false;
            rb.isKinematic = true;
            
            animator.SetBool("isDriving", true);
            
            // Aggiungi questa linea per far partire la musica
            AudioManager.Instance.StartCarMusic();
            
            // Mostra la UI della musica
            MusicUIController.Instance.ShowMusicUI(true);
            
            yield return new WaitForSeconds(0.1f);
            
            carController.EnterCar(this);
        }
    }

    private IEnumerator EnterUFO()
    {
        // Rotate the UFO glass on the Z axis by -80 degrees
        Quaternion targetRotation = ufoGlass.localRotation * Quaternion.Euler(0, 0, -80);
        float rotationDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            ufoGlass.localRotation = Quaternion.Slerp(ufoGlass.localRotation, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ufoGlass.localRotation = targetRotation; // Ensure the final rotation is exact
        isInUFO = true;
    }

    private IEnumerator SitInUFO()
    {
        // Smoothly transition the player to the UFO seat position
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 endPosition = ufoSeatTrigger.position;
        Quaternion endRotation = ufoSeatTrigger.rotation;
        float transitionDuration = 0.5f; // Duration of the transition
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / transitionDuration);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position and rotation are set
        transform.position = endPosition;
        transform.rotation = endRotation;

        transform.SetParent(ufo.transform); // Parent the player to the UFO
        rb.isKinematic = true; // Disable physics for the player
        rb.linearVelocity = Vector3.zero; // Reset velocitylocity
        rb.angularVelocity = Vector3.zero; // Reset angular velocity
        animator.SetBool("isDriving", true); // Start the driving animation

        // Close the UFO glass
        StartCoroutine(CloseUFOGlass());

        // Switch control to the UFO
        ufo.GetComponent<UFOController>().EnterUFO(this);
    }

    private IEnumerator CloseUFOGlass()
    {
        // Rotate the UFO glass back to its original position
        Quaternion targetRotation = ufoGlass.localRotation * Quaternion.Euler(0, 0, 80);
        float rotationDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            ufoGlass.localRotation = Quaternion.Slerp(ufoGlass.localRotation, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ufoGlass.localRotation = targetRotation; // Ensure the final rotation is exact
    }

    public void ExitCar()
    {
        if (isExitingCar) return;
        isExitingCar = true;
        isInCar = false;
        
        // Ferma completamente la musica quando esci dalla macchina
        AudioManager.Instance.StopCarMusic();
        
        // Nascondi la UI della musica
        MusicUIController.Instance.ShowMusicUI(false);
        
        animator.SetBool("isDriving", false);
        
        // Riattiva il movimento del player
        enabled = true;
        rb.isKinematic = false;
        
        // Ripristina il controllo della camera
        xRotation = 0f;
        yRotation = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Ripristina la camera alla vista del character
        UpdateCameraForCharacter();
        
        car.GetComponent<CarController>().ExitCar();
        // Non azzeriamo più il riferimento alla macchina
        // car = null; // Rimuovi questa linea
        nearCar = true; // Manteniamo lo stato "vicino alla macchina"
        isExitingCar = false;
    }

    private void UpdateCameraForCharacter()
    {
        // Resetta la posizione della camera relativa al player
        Vector3 desiredPosition = transform.position + Vector3.up * cameraHeightOffset;
        cameraTransform.position = desiredPosition - transform.forward * cameraOffset.z;
        
        // Aggiorna la rotazione della camera
        Quaternion cameraRotation = Quaternion.Euler(xRotation + cameraRotationOffset.x, 
                                                   yRotation + cameraRotationOffset.y, 
                                                   cameraRotationOffset.z);
        cameraTransform.rotation = cameraRotation;
    }

    public void ExitUFO()
    {
        animator.SetBool("isDriving", false); // Stop the driving animation
        transform.SetParent(null); // Unparent the player from the UFO
        rb.isKinematic = false; // Enable physics for the player
        rb.AddForce(ufo.transform.forward * 5f, ForceMode.Impulse); // Eject the player from the UFO
        ufo.GetComponent<UFOController>().ExitUFO();
        ufo = null;
        ufoGlass = null;
        ufoSeatTrigger = null;
        isInUFO = false;
        enabled = true; // Enable player controls
    }

    public void EquipJetpack(Jetpack newJetpack)
    {
        if (jetpack != null)
        {
            jetpack.EjectJetpack();
        }
        
        // Reset della fisica del player prima di equipaggiare
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        jetpack = newJetpack;
        jetpack.EquipJetpack(rb, transform, cameraTransform);
    }

    public void RemoveJetpack()
    {
        if (jetpack != null)
        {
            Debug.Log("Removing jetpack");
            
            // Prima di tutto, ripristina lo stato del player
            rb.useGravity = true;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            
            // Memorizza il jetpack e rimuovi subito il riferimento
            Jetpack jetpackToRemove = jetpack;
            jetpack = null;
            
            // Ora espelli il jetpack
            jetpackToRemove.ForceEject();
        }
    }
}