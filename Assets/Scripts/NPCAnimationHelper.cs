using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class NPCAnimationHelper : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationSpeed = 1.0f;
    public float dampTime = 0.1f;
    public string speedParameter = "Speed";
    public string isWalkingParameter = "IsWalking";
    
    [Header("Movement Thresholds")]
    public float movementThreshold = 0.1f;
    public float destinationThreshold = 0.5f;
    
    // Riferimenti ai componenti
    private Animator animator;
    private NavMeshAgent agent;
    
    // Variabili di stato
    private bool previouslyMoving = false;
    private Vector3 lastPosition;
    private float currentSpeed = 0f;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        if (animator == null || agent == null)
        {
            Debug.LogError("NPCAnimationHelper richiede Animator e NavMeshAgent!", this);
            enabled = false;
            return;
        }
        
        // Configurazione iniziale
        lastPosition = transform.position;
        
        // Assicurati che l'animatore non usi root motion
        animator.applyRootMotion = false;
        
        // Imposta la velocità di animazione
        animator.speed = animationSpeed;
    }
    
    private void Update()
    {
        UpdateAnimatorParameters();
    }
    
    private void UpdateAnimatorParameters()
    {
        // Calcola la velocità effettiva in base al movimento
        Vector3 delta = transform.position - lastPosition;
        float speed = delta.magnitude / Time.deltaTime;
        lastPosition = transform.position;
        
        // Applica smoothing alla velocità
        currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 5f);
        
        // Verifica se l'NPC si sta muovendo
        bool isMoving = currentSpeed > movementThreshold || 
                       (agent.hasPath && agent.remainingDistance > destinationThreshold);
        
        // Aggiorna i parametri dell'animatore
        if (animator != null)
        {
            // Aggiorna il parametro della velocità con smoothing
            animator.SetFloat(speedParameter, currentSpeed, dampTime, Time.deltaTime);
            
            // Aggiorna il bool dello stato di movimento solo quando cambia
            if (isMoving != previouslyMoving)
            {
                animator.SetBool(isWalkingParameter, isMoving);
                previouslyMoving = isMoving;
            }
            
            // Debug
            if (isMoving != previouslyMoving)
            {
                Debug.Log($"[{gameObject.name}] Stato movimento: {(isMoving ? "In movimento" : "Fermo")}, " +
                         $"Velocità: {currentSpeed:F2}, Distanza rimanente: {agent.remainingDistance:F2}");
            }
        }
    }
    
    // Metodo per forzare l'animazione di idle
    public void ForceIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(isWalkingParameter, false);
            animator.SetFloat(speedParameter, 0f);
            previouslyMoving = false;
        }
    }
    
    // Metodo per forzare l'animazione di camminata
    public void ForceWalkAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(isWalkingParameter, true);
            previouslyMoving = true;
        }
    }
}
