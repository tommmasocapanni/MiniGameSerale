using UnityEngine;

public class JetpackPickup : MonoBehaviour
{
    private Jetpack parentJetpack;

    void Start()
    {
        // Get reference to parent Jetpack component
        parentJetpack = GetComponentInParent<Jetpack>();
        if (parentJetpack == null)
        {
            Debug.LogError("JetpackPickup must be a child of a GameObject with Jetpack component!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && parentJetpack != null && !parentJetpack.IsEquipped())
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.EquipJetpack(parentJetpack);
            }
        }
    }
}