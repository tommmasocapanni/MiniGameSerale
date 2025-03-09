using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPCSetup : MonoBehaviour
{
    private void Awake()
    {
        // Configura il Rigidbody per funzionare bene con NavMeshAgent
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Importante: impedisce alla fisica di interferire con NavMesh
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Configura il CapsuleCollider se necessario
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            // Regola le dimensioni in base al tuo modello
            // Questi sono valori di esempio per un personaggio umanoide
            capsuleCollider.height = 2.0f;
            capsuleCollider.radius = 0.5f;
            capsuleCollider.center = new Vector3(0, 1.0f, 0);
        }
    }
}
