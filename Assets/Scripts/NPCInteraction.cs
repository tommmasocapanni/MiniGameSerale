using UnityEngine;
using UnityEngine.Events;

public class NPCInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRadius = 2f;
    public string playerTag = "Player";
    public KeyCode interactionKey = KeyCode.E;
    public string promptMessage = "Premi E per interagire";
    
    [Header("Events")]
    public UnityEvent onInteractionStart;
    public UnityEvent onInteractionEnd;
    
    private bool playerInRange = false;
    private GameObject currentPlayer;
    private NPCAudio audioHandler;
    
    private void Start()
    {
        audioHandler = GetComponent<NPCAudio>();
    }
    
    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            Interact();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            currentPlayer = other.gameObject;
            
            // Mostra un messaggio al giocatore (puoi personalizzare come preferisci)
            Debug.Log(promptMessage);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            currentPlayer = null;
            
            // Nascondi il messaggio
        }
    }
    
    private void Interact()
    {
        // Esegui l'evento di interazione
        onInteractionStart.Invoke();
        
        // Riproduci suono di interazione se disponibile
        if (audioHandler != null)
        {
            audioHandler.PlayInteractionSound();
        }
        
        // Ferma temporaneamente l'NPC se si sta muovendo
        SimplifiedNPC movementController = GetComponent<SimplifiedNPC>();
        if (movementController != null)
        {
            // Pausa il movimento per 3 secondi durante l'interazione
            movementController.ForzaArresto();
            
            // Opzionale: dopo 3 secondi, riprendi il movimento
            Invoke("RiprendiMovimento", 3.0f);
        }
    }
    
    // Nuovo metodo per riprendere il movimento dopo un'interazione
    private void RiprendiMovimento()
    {
        SimplifiedNPC movementController = GetComponent<SimplifiedNPC>();
        if (movementController != null)
        {
            movementController.ForzaMovimento();
        }
    }
    
    // Per riflettere visivamente il raggio di interazione
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
    
    // Aggiungi un trigger collider per l'interazione
    private void OnValidate()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
        }
        
        col.radius = interactionRadius;
        col.center = new Vector3(0, 1f, 0);  // Posiziona al centro del personaggio
    }
}
