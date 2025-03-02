using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Vector3 equipPosition = Vector3.zero;
    [SerializeField] private Vector3 equipRotation = Vector3.zero;
    [SerializeField] private float fireRate = 0.5f; // Tempo tra ogni sparo in secondi
    
    private bool isCollected = false;
    private Transform playerHandTransform;
    private Animator playerAnimator;
    private bool canFire = true;
    private float nextFireTime = 0f;

    private void Start()
    {
        // Assicurati che l'oggetto abbia un trigger collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"Aggiungi un Collider a {gameObject.name}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            // Cerca il punto di riferimento della mano del player
            playerHandTransform = other.transform.Find("Hand");
            playerAnimator = other.GetComponent<Animator>();
            
            if (playerHandTransform == null)
            {
                Debug.LogWarning("Aggiungi un GameObject 'Hand' come figlio del Player!");
                return;
            }
            if (playerAnimator == null)
            {
                Debug.LogWarning("Player non ha un componente Animator!");
                return;
            }

            EquipItem();
        }
    }

    private void Update()
    {
        if (isCollected && canFire)
        {
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime) // Cambiato da GetMouseButtonDown a GetMouseButton
            {
                Fire();
                nextFireTime = Time.time + fireRate; // Imposta il prossimo momento in cui potrà sparare
            }
        }
    }

    private void Fire()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("isFiring"); // Cambiato da SetBool a SetTrigger
            Debug.Log("Firing!");
        }
    }

    private void EquipItem()
    {
        isCollected = true;
        
        // Disattiva physics e collider
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.enabled = false;
        }

        // Parent l'oggetto alla mano e posizionalo
        transform.SetParent(playerHandTransform);
        transform.localPosition = equipPosition;
        transform.localEulerAngles = equipRotation;

        Debug.Log($"{itemName} equipaggiato!");
        canFire = true;
    }
}
