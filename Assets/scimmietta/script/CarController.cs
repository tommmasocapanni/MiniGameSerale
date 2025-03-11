using UnityEngine;
using System.Collections;  // Aggiunto per IEnumerator

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float motorForce = 20f;
    public float maxSpeed = 20f;
    public float turnSpeed = 100f;
    public float brakeForce = 30f;
    public float downForce = 1000f;  // Add this new field
    public float jumpForce = 3000f;  // Aumentato per rendere il salto più evidente
    public float jumpCooldown = 1f;  // Tempo di attesa tra i salti
    public LayerMask groundLayer; // Per il ground check
    private bool canJump = true;     // Flag per il cooldown del salto

    [Header("Wheel Transforms")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [Header("Wheel Settings")]
    [SerializeField] protected float wheelBaseRotationY = -90f; // Rotazione base delle ruote sull'asse Y
    [SerializeField] protected bool invertWheelRotation = false; // Per invertire la rotazione se necessario
    [SerializeField] protected float steeringAngleMultiplier = 10f; // Moltiplicatore per l'angolo di sterzata
    [SerializeField] protected bool useZAxisRotation = false; // Per ruotare sull'asse Z invece che X
    protected float frontWheelsRotation = 0f; // Rinominato per essere più generico

    [Header("Car Components")]
    public Transform seatTrigger;
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(0, 5, -10);

    [Header("Camera Settings")]
    public float cameraReturnSpeed = 2f;
    public float cameraRotationSpeed = 2f;
    public float cameraTurnEffect = 0.5f;
    public float cameraYawOffset = 180f;   // Offset di rotazione orizzontale
    public float cameraPitchOffset = 1f;   // Offset di rotazione verticale (era 15f)
    private float targetCameraRotationX;    // Rimuovi l'inizializzazione fissa
    private float targetCameraRotationY = 0f;
    private float lastMouseMoveTime;
    private float mouseIdleTime = 2f; // Tempo dopo il quale la camera torna al centro

    [Header("Drift Settings")]
    public float driftPowerGain = 0.5f;  // Ridotto per rendere più graduale l'accumulo
    public float driftSteeringMultiplier = 1.5f;
    public Color[] driftColors = new Color[] { Color.blue, Color.yellow, Color.red };
    public float[] driftLevelThresholds = new float[] { 2f, 4f, 6f };  // Tempo in secondi per ogni livello
    public ParticleSystem[] wheelParticles;
    private bool isDrifting = false;
    private float driftPower = 0f;
    private float driftTimer = 0f;  // Timer per tracciare la durata del drift
    private int driftDirection = 0;
    private int driftLevel = 0;

    protected float horizontalInput, verticalInput;
    protected bool isBreaking;
    protected PlayerController playerController;
    protected bool isPlayerInCar = false;
    protected bool isExitingCar = false;
    protected Rigidbody rb;
    protected Collider carCollider;
    protected float cameraRotationX = 0f;
    protected float cameraRotationY = 0f;
    protected float currentSpeed = 0f;
    protected bool isGrounded;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        carCollider = GetComponent<Collider>();
        
        // Configure Rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
        rb.mass = 1500f;
        rb.linearDamping = 1f;
        rb.angularDamping = 5f;
        
        // Align car with seatTrigger initially
        transform.rotation = seatTrigger.rotation;
        SetInitialWheelRotations();
        targetCameraRotationX = cameraPitchOffset; // Inizializza qui
    }

    private void SetInitialWheelRotations()
    {
        // Set base Y rotation for all wheels
        frontLeftWheel.localRotation = Quaternion.Euler(0, wheelBaseRotationY, 0);
        frontRightWheel.localRotation = Quaternion.Euler(0, wheelBaseRotationY, 0);
        rearLeftWheel.localRotation = Quaternion.Euler(0, wheelBaseRotationY, 0);
        rearRightWheel.localRotation = Quaternion.Euler(0, wheelBaseRotationY, 0);
    }

    private void FixedUpdate()
    {
        if (!isPlayerInCar) return;

        GetInput();
        HandleMovement();
        RotateWheels();
        ApplyDownforce();
    }

    private void Update()
    {
        if (!isPlayerInCar) return;

        // Handle camera
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            lastMouseMoveTime = Time.time;
            cameraRotationX = Mathf.Clamp(cameraRotationX - mouseY * cameraRotationSpeed, -60f, 60f);
            cameraRotationY += mouseX * cameraRotationSpeed;
        }
        else
        {
            // Se il mouse non si muove per un po', riporta la camera al centro
            if (Time.time - lastMouseMoveTime > mouseIdleTime)
            {
                // Usa la direzione del seatTrigger + 180 gradi come riferimento
                targetCameraRotationY = seatTrigger.eulerAngles.y + cameraYawOffset;
                
                // Applica un effetto di inclinazione nelle curve (invertito per la rotazione di 180°)
                targetCameraRotationY -= horizontalInput * cameraTurnEffect * currentSpeed;
                
                // Interpola smoothly verso la posizione target
                cameraRotationX = Mathf.LerpAngle(cameraRotationX, cameraPitchOffset, Time.deltaTime * cameraReturnSpeed);
                cameraRotationY = Mathf.LerpAngle(cameraRotationY, targetCameraRotationY, Time.deltaTime * cameraReturnSpeed);
            }
        }

        // Applica la rotazione finale della camera
        Quaternion rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
        cameraTransform.position = transform.position + rotation * cameraOffset;
        cameraTransform.LookAt(transform.position + Vector3.up * 2);

        // Controlla se il giocatore preme "R" per attivare/disattivare la musica
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("CarController: Toggling music with R key");
            AudioManager.Instance.PlayPause();
            // Aggiorna l'UI quando si preme R
            MusicUIController.Instance.UpdatePlayPauseButton(AudioManager.Instance.isPlaying);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ExitCar();
        }
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.LeftControl); // Cambio tasto freno a LeftControl

        // Debug del salto
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"Space pressed. CanJump: {canJump}, IsGrounded: {isGrounded}");
            if (canJump && isGrounded)
            {
                Jump();
            }
        }

        // VERIFICA: Aggiungi debug per input drift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log($"Shift pressed - Speed: {currentSpeed}, Horizontal: {horizontalInput}");
        }
        
        // Modifica gestione drift input
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);  // Cambiato da GetKeyDown a GetKey
        
        if (shiftPressed && !isDrifting && Mathf.Abs(horizontalInput) > 0.1f)
        {
            StartDrift();
        }
        else if (!shiftPressed && isDrifting)  // Cambiato da GetKeyUp a !shiftPressed
        {
            EndDrift();
        }

        // Debug continuo dello stato del drift
        if (isDrifting && Time.frameCount % 60 == 0)  // Log ogni secondo
        {
            Debug.Log($"Drift Active - Shift Held: {shiftPressed}, Power: {driftPower:F2}, Level: {driftLevel}");
        }
    }

    protected virtual void HandleMovement()
    {
        if (isBreaking)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, brakeForce * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, verticalInput * maxSpeed, motorForce * Time.fixedDeltaTime);
        }

        // Always use seatTrigger's forward as the reference for movement
        Vector3 movement = seatTrigger.forward * currentSpeed;
        movement.y = rb.linearVelocity.y;
        rb.linearVelocity = movement;

        // Gestione della rotazione
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float turnAmount = horizontalInput * turnSpeed * Time.fixedDeltaTime;
            if (currentSpeed < 0) turnAmount = -turnAmount;

            // Verifica che il drift sia ancora valido
            if (isDrifting && !Input.GetKey(KeyCode.LeftShift))
            {
                EndDrift();  // Failsafe aggiuntivo
            }

            // Applica il moltiplicatore di steering durante il drift
            if (isDrifting)
            {
                turnAmount *= driftSteeringMultiplier;
                
                // VERIFICA: Migliora calcolo drift power
                float powerControl = (driftDirection == 1) ? 
                    Mathf.Lerp(0.2f, 1f, Mathf.Abs(horizontalInput)) : 
                    Mathf.Lerp(1f, 0.2f, Mathf.Abs(horizontalInput));
                    
                driftPower += powerControl * driftPowerGain * Time.fixedDeltaTime;
                
                // Debug per monitorare i valori
                if (Time.frameCount % 30 == 0) // Log ogni 30 frames per non spammare
                {
                    Debug.Log($"Drift - Power: {driftPower:F2}, Direction: {driftDirection}, Control: {powerControl:F2}");
                }
                
                UpdateDriftLevel();
            }

            transform.RotateAround(seatTrigger.position, Vector3.up, turnAmount);
        }
        else if (isDrifting)  // Se la velocità è troppo bassa, termina il drift
        {
            EndDrift();
        }
    }

    protected virtual void RotateWheels()
    {
        float wheelRotation = currentSpeed * 360 * Time.fixedDeltaTime * (invertWheelRotation ? -1 : 1);
        
        // Update accumulated rotation
        frontWheelsRotation += wheelRotation;
        
        // Prepare the rotation vectors based on the selected axis
        Vector3 rollingAxis = useZAxisRotation ? new Vector3(0, 0, 1) : new Vector3(1, 0, 0);
        
        // Rotate rear wheels (only rolling motion)
        if (useZAxisRotation)
        {
            rearLeftWheel.localRotation = Quaternion.Euler(0, wheelBaseRotationY, frontWheelsRotation);
            rearRightWheel.localRotation = Quaternion.Euler(0, wheelBaseRotationY, frontWheelsRotation);
        }
        else
        {
            rearLeftWheel.localRotation = Quaternion.Euler(frontWheelsRotation, wheelBaseRotationY, 0);
            rearRightWheel.localRotation = Quaternion.Euler(frontWheelsRotation, wheelBaseRotationY, 0);
        }

        // Calculate steering angle
        float steerAngle = horizontalInput * steeringAngleMultiplier;

        // Apply rotation to front wheels (rolling + steering)
        if (useZAxisRotation)
        {
            frontLeftWheel.localRotation = Quaternion.Euler(0, wheelBaseRotationY + steerAngle, frontWheelsRotation);
            frontRightWheel.localRotation = Quaternion.Euler(0, wheelBaseRotationY + steerAngle, frontWheelsRotation);
        }
        else
        {
            frontLeftWheel.localRotation = Quaternion.Euler(frontWheelsRotation, wheelBaseRotationY + steerAngle, 0);
            frontRightWheel.localRotation = Quaternion.Euler(frontWheelsRotation, wheelBaseRotationY + steerAngle, 0);
        }
    }

    private void ApplyDownforce()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f, groundLayer);
        
        if (isGrounded)
        {
            float currentHeight = hit.distance;
            rb.AddForce(-transform.up * downForce * (currentHeight > 0.5f ? 2f : 1f));
        }
    }

    private void Jump()
    {
        Debug.Log("Car Jumping!");
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        canJump = false;
        StartCoroutine(JumpCooldown());
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    private void StartDrift()
    {
        isDrifting = true;
        driftDirection = horizontalInput > 0 ? 1 : -1;
        driftPower = 0f;
        driftTimer = 0f;
        driftLevel = 0;
        
        Debug.Log($"Drift Started - Direction: {driftDirection}");
        
        // Attiva le particelle con il colore iniziale
        foreach (ParticleSystem ps in wheelParticles)
        {
            var main = ps.main;
            main.startColor = driftColors[0];
            ps.Play();
        }
    }

    private void EndDrift()
    {
        if (!isDrifting) return;  // Previene chiamate multiple
        
        isDrifting = false;
        Debug.Log("Drift Ended - Final Level: " + driftLevel);
        
        // VERIFICA: Migliora calcolo boost finale
        if (driftLevel > 0)
        {
            float boostMultiplier = 1f + (driftLevel * 0.2f);
            float finalSpeed = currentSpeed * boostMultiplier;
            Debug.Log($"Drift End - Level: {driftLevel}, Boost: x{boostMultiplier:F2}, Final Speed: {finalSpeed:F2}");
            currentSpeed = finalSpeed;
        }

        // Resetta i valori
        driftPower = 0f;
        driftTimer = 0f;
        driftLevel = 0;

        // Ferma le particelle
        foreach (ParticleSystem ps in wheelParticles)
        {
            ps.Stop();
        }
    }

    private void UpdateDriftLevel()
    {
        if (!isDrifting) return;

        driftTimer += Time.deltaTime;
        
        // Aggiorna il livello di drift basato sul tempo
        for (int i = driftLevelThresholds.Length - 1; i >= 0; i--)
        {
            if (driftTimer >= driftLevelThresholds[i] && driftLevel != i + 1)
            {
                driftLevel = i + 1;
                UpdateDriftParticles(i);
                // Aumenta la potenza del drift in base al livello
                driftPower = driftTimer * (1 + driftLevel * 0.5f);
                break;
            }
        }
    }

    private void UpdateDriftParticles(int colorIndex)
    {
        foreach (ParticleSystem ps in wheelParticles)
        {
            var main = ps.main;
            main.startColor = driftColors[colorIndex];
        }
    }

    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public virtual void EnterCar(PlayerController player)
    {
        if (player == null)
        {
            Debug.LogError("Player reference is null in EnterCar!");
            return;
        }

        isPlayerInCar = true;
        playerController = player;
        
        if (carCollider == null)
        {
            carCollider = GetComponent<Collider>();
        }
        
        Physics.IgnoreCollision(playerController.GetComponent<Collider>(), carCollider, true);
        
        // Disattiva la fisica del player
        playerController.rb.isKinematic = true;
        
        // Fissa il player al seatTrigger
        playerController.transform.SetParent(seatTrigger);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;
        
        // Mostra la UI della musica
        MusicUIController.Instance.ShowMusicUI(true);
        
        // Reset camera rotation con l'offset configurabile
        lastMouseMoveTime = Time.time;
        cameraRotationX = cameraPitchOffset;
        cameraRotationY = seatTrigger.eulerAngles.y + cameraYawOffset;
        targetCameraRotationY = cameraRotationY;
        
        // Align car with seatTrigger
        transform.rotation = seatTrigger.rotation;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerInCar)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null && !pc.IsInAnyCar())
            {
                Debug.Log("OnTriggerEnter: Player near car");
                pc.nearCar = true;
                pc.nearestCar = gameObject;
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null && pc.nearestCar == gameObject)
            {
                Debug.Log("OnTriggerExit: Player left car area");
                pc.nearCar = false;
                pc.nearestCar = null;
            }
        }
    }

    public virtual void ExitCar()
    {
        if (isExitingCar) return;
        isExitingCar = true;

        isPlayerInCar = false;
        if (playerController != null)
        {
            // Riattiva la fisica del player
            playerController.rb.isKinematic = false;
            
            // Posiziona il player di lato alla macchina
            playerController.transform.SetParent(null);
            playerController.transform.position = seatTrigger.position + seatTrigger.right * 2;
            playerController.transform.rotation = Quaternion.Euler(0, seatTrigger.eulerAngles.y, 0);
            
            // Riattiva tutti i controlli del player
            playerController.enabled = true;
            playerController.isInCar = false; // Resetta esplicitamente lo stato del player
            
            // Disattiva animazione di guida
            playerController.animator.SetBool("isDriving", false);
            
            // Riattiva le collisioni
            Physics.IgnoreCollision(playerController.GetComponent<Collider>(), carCollider, false);
            
            // Il player rimane vicino alla macchina
            playerController.nearCar = true;
            playerController.nearestCar = gameObject;
        }

        rb.isKinematic = false;
        isExitingCar = false;
        
        // Audio e UI
        AudioManager.Instance.StopCarMusic();
        MusicUIController.Instance.ShowMusicUI(false);
    }

    private IEnumerator ResetPlayerControllerAfterExit()
    {
        yield return new WaitForFixedUpdate();
        
        if (playerController != null && playerController.nearCar)
        {
            yield break;
        }
        
        if (playerController != null)
        {
            playerController.nearCar = false;
            playerController.nearestCar = null;
            playerController = null;
        }
    }
}
