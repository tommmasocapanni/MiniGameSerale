using UnityEngine;

public class Jetpack : MonoBehaviour
{
    public float jetpackForce = 15f;
    public float maxFuel = 100f;
    public float fuelConsumptionRate = 20f;
    public float fuelRechargeRate = 10f;
    public ParticleSystem leftThruster;
    public ParticleSystem rightThruster;

    private float currentFuel;
    private bool isEquipped;
    private Rigidbody playerRb;
    private Collider jetpackCollider;
    private Rigidbody jetpackRb;
    private Transform cameraTransform;

    void Start()
    {
        currentFuel = maxFuel;
        if (leftThruster) leftThruster.Stop();
        if (rightThruster) leftThruster.Stop();
        jetpackCollider = GetComponent<Collider>();
        jetpackRb = GetComponent<Rigidbody>();
        jetpackRb.isKinematic = true; // Ensure the Rigidbody is kinematic initially
    }

    void Update()
    {
        if (!isEquipped || playerRb == null || cameraTransform == null) return;

        Vector3 forceDirection = Vector3.zero;
        float targetRotationX = 0f;
        float targetRotationZ = 0f;

        if (Input.GetKey(KeyCode.Space) && currentFuel > 0)
        {
            forceDirection += Vector3.up;
            currentFuel -= fuelConsumptionRate * Time.deltaTime;
            currentFuel = Mathf.Max(0, currentFuel);
        }

        if (Input.GetKey(KeyCode.W))
        {
            forceDirection += cameraTransform.forward;
            targetRotationX = 30f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            forceDirection -= cameraTransform.forward;
            targetRotationX = -30f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            forceDirection -= cameraTransform.right;
            targetRotationZ = -30f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            forceDirection += cameraTransform.right;
            targetRotationZ = 30f;
        }

        if (forceDirection != Vector3.zero)
        {
            playerRb.AddForce(forceDirection.normalized * jetpackForce, ForceMode.Acceleration);
            playerRb.useGravity = false;

            if (!leftThruster.isPlaying) leftThruster.Play();
            if (!rightThruster.isPlaying) rightThruster.Play();
        }
        else
        {
            playerRb.useGravity = true;

            if (leftThruster.isPlaying) leftThruster.Stop();
            if (rightThruster.isPlaying) rightThruster.Stop();

            currentFuel += fuelRechargeRate * Time.deltaTime;
            currentFuel = Mathf.Min(currentFuel, maxFuel);
        }

        // Smoothly interpolate the rotation
        float smoothRotationX = Mathf.LerpAngle(playerRb.rotation.eulerAngles.x, targetRotationX, Time.deltaTime * 5f);
        float smoothRotationZ = Mathf.LerpAngle(playerRb.rotation.eulerAngles.z, targetRotationZ, Time.deltaTime * 5f);
        playerRb.rotation = Quaternion.Euler(smoothRotationX, playerRb.rotation.eulerAngles.y, smoothRotationZ);

        if (Input.GetKeyDown(KeyCode.E))
        {
            EjectJetpack();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isEquipped && other.CompareTag("Player"))
        {
            Debug.Log("Tentativo di raccolta jetpack");
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Resetta la rotazione del jetpack prima di equipaggiarlo
                jetpackRb.angularVelocity = Vector3.zero;
                player.EquipJetpack(this);
            }
        }
    }

    public void EquipJetpack(Rigidbody playerRigidbody, Transform playerTransform, Transform cameraTransform)
    {
        Debug.Log("Equipaggiamento jetpack...");
        
        // Reset completo della fisica
        jetpackRb.linearVelocity = Vector3.zero;
        jetpackRb.angularVelocity = Vector3.zero;
        jetpackRb.isKinematic = true;
        jetpackRb.interpolation = RigidbodyInterpolation.None;
        jetpackRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        jetpackRb.constraints = RigidbodyConstraints.None;
        
        // Reset del collider
        jetpackCollider.enabled = false;
        jetpackCollider.isTrigger = true;
        
        // Setup delle trasformazioni
        transform.SetParent(null); // Temporaneamente rimuovi il parent per resettare
        transform.rotation = Quaternion.identity; // Reset completo della rotazione
        transform.SetParent(playerTransform); // Riassegna il parent
        transform.localPosition = new Vector3(0, 0.8f, -0.35f);
        transform.localRotation = Quaternion.Euler(-90f, -90f, 0);
        
        // Setup delle referenze
        playerRb = playerRigidbody;
        this.cameraTransform = cameraTransform;
        isEquipped = true;

        // Setup della fisica del player
        playerRb.freezeRotation = true;
        playerRb.useGravity = false;
        playerRb.linearVelocity = Vector3.zero;
        
        Debug.Log("Jetpack equipaggiato con successo");
    }

    public void EjectJetpack()
    {
        Debug.Log("Starting jetpack ejection...");

        if (!isEquipped || playerRb == null)
        {
            Debug.LogError("Cannot eject: jetpack not properly equipped");
            return;
        }

        // 1. Salva i riferimenti necessari
        Transform oldParent = transform.parent;
        Vector3 currentWorldPos = transform.position;
        Vector3 currentWorldRot = transform.eulerAngles;
        Rigidbody oldPlayerRb = playerRb;

        // 2. Rimuovi il parent e mantieni la posizione mondiale
        transform.SetParent(null);
        transform.position = currentWorldPos;
        transform.eulerAngles = currentWorldRot;

        // 3. Configura fisica e collisioni
        jetpackRb.isKinematic = false;
        jetpackRb.useGravity = true;
        jetpackCollider.enabled = true;
        jetpackCollider.isTrigger = true;

        // 4. Applica le forze di espulsione
        Vector3 ejectDirection = (transform.position - oldPlayerRb.transform.position).normalized;
        jetpackRb.linearVelocity = oldPlayerRb.linearVelocity;
        jetpackRb.AddForce((ejectDirection + Vector3.up) * 5f, ForceMode.Impulse);
        jetpackRb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);

        // 5. Reset stati e referenze
        isEquipped = false;
        playerRb = null;
        cameraTransform = null;

        // 6. Ferma effetti
        if (leftThruster) leftThruster.Stop();
        if (rightThruster) rightThruster.Stop();

        Debug.Log($"Jetpack ejected successfully. Was child of: {oldParent.name}");
    }

    public void ForceEject()
    {
        Debug.Log("Force ejecting jetpack");
        
        // 1. Disattiva immediatamente il controllo
        isEquipped = false;
        
        // 2. Salva e pulisci i riferimenti
        Transform oldParent = transform.parent;
        Rigidbody oldPlayerRb = playerRb;
        Vector3 playerVelocity = oldPlayerRb != null ? oldPlayerRb.linearVelocity : Vector3.zero;
        
        playerRb = null;
        cameraTransform = null;
        
        // 3. Stacca dal parent e imposta posizione/rotazione
        Vector3 currentWorldPos = transform.position;
        Vector3 currentWorldRot = transform.eulerAngles;
        transform.SetParent(null);
        transform.position = currentWorldPos + Vector3.back * 1f; // Sposta leggermente indietro
        transform.eulerAngles = currentWorldRot;
        
        // 4. Riattiva fisica e collisioni
        jetpackRb.isKinematic = false;
        jetpackRb.useGravity = true;
        jetpackRb.constraints = RigidbodyConstraints.None;
        jetpackCollider.enabled = true;
        jetpackCollider.isTrigger = false; // Importante: disattiva il trigger per le collisioni fisiche
        
        // 5. Applica forze di espulsione
        if (oldParent != null)
        {
            // Mantieni la velocità orizzontale del player
            jetpackRb.linearVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);
            Vector3 ejectDir = -oldParent.forward; // Usa la direzione opposta al forward del player
            jetpackRb.AddForce(ejectDir * 5f + Vector3.up * 3f, ForceMode.Impulse);
            jetpackRb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);
        }
        
        // 6. Spegni i propulsori
        if (leftThruster) leftThruster.Stop();
        if (rightThruster) rightThruster.Stop();
        
        Debug.Log("Jetpack ejected from " + oldParent?.name);
    }

    public bool HasFuel()
    {
        return currentFuel > 0;
    }

    public void ConsumeFuel(float amount)
    {
        currentFuel -= amount;
        currentFuel = Mathf.Max(0, currentFuel);
    }

    public bool IsEquipped()
    {
        return isEquipped;
    }
}