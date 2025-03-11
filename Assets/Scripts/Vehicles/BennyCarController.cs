using UnityEngine;
using System.Collections;

public class BennyCarController : CarController
{
    protected override void Start()
    {
        // Configura prima del base.Start()
        wheelBaseRotationY = 0f;
        useZAxisRotation = true;
        
        // Configura il Rigidbody prima di tutto
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
            transform.rotation = seatTrigger.rotation; // Allinea con il seat trigger
        }

        base.Start();
    }

    protected override void HandleMovement()
    {
        // Assicurati che il movimento sia completamente fermo se non c'è input
        if (Mathf.Abs(verticalInput) < 0.01f && Mathf.Abs(currentSpeed) < 0.1f)
        {
            currentSpeed = 0f;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // Gestisci la rotazione in retromarcia
        if (currentSpeed < -0.1f)
        {
            // Inverti la rotazione quando vai in retromarcia e riduci l'effetto
            horizontalInput *= -0.5f;
            // Limita la velocità angolare in retromarcia
            rb.angularVelocity = new Vector3(
                rb.angularVelocity.x,
                Mathf.Clamp(rb.angularVelocity.y, -1f, 1f),
                rb.angularVelocity.z
            );
        }

        base.HandleMovement();
    }

    public override void EnterCar(PlayerController player)
    {
        // Reset completo della fisica quando il player entra
        if (rb != null)
        {
            rb.isKinematic = true; // Temporaneamente rendi il veicolo kinematic
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            currentSpeed = 0f;
            
            // Assicurati che la rotazione sia corretta
            transform.rotation = Quaternion.Euler(0, seatTrigger.eulerAngles.y, 0);
            
            // Dopo un breve delay, riattiva la fisica
            StartCoroutine(ReenablePhysics());
        }
        
        base.EnterCar(player);
    }

    private IEnumerator ReenablePhysics()
    {
        yield return new WaitForSeconds(0.1f);
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
        }
    }
}
