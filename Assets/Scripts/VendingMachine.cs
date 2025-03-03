using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject itemPrefab; // Il prefab dell'oggetto da spawnare
    [SerializeField] private Transform spawnPoint; // Il punto di spawn (Empty GameObject figlio)
    
    private bool playerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            DispenseItem();
        }
    }

    private void DispenseItem()
    {
        if (itemPrefab != null && spawnPoint != null)
        {
            GameObject dispensedItem = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
            
            // Aggiungi una piccola forza casuale per simulare la caduta
            if (dispensedItem.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                float randomX = Random.Range(-0.5f, 0.5f);
                float randomZ = Random.Range(-0.5f, 0.5f);
                rb.AddForce(new Vector3(randomX, 0, randomZ), ForceMode.Impulse);
            }
        }
    }
}
