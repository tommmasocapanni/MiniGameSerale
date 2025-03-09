using UnityEngine;
using UnityEngine.AI;

public class NPCDiagnostics : MonoBehaviour
{
    public bool fixNavMeshAgent = true;
    public bool fixConflictingScripts = true;
    public bool logDiagnostics = true;
    
    void Start()
    {
        // Verifica la presenza di script di movimento multipli che potrebbero essere in conflitto
        Component[] movementScripts = GetComponents<MonoBehaviour>();
        int movementScriptCount = 0;
        MonoBehaviour activeScript = null;
        
        foreach (MonoBehaviour script in movementScripts)
        {
            if (script != null && script.enabled)
            {
                string scriptName = script.GetType().Name;
                if (scriptName.Contains("Movement") || scriptName.Contains("Animation"))
                {
                    movementScriptCount++;
                    activeScript = script;
                    if (logDiagnostics)
                        Debug.Log($"Script di movimento trovato: {scriptName} (abilitato: {script.enabled})");
                }
            }
        }
        
        if (movementScriptCount > 1 && fixConflictingScripts)
        {
            FixMultipleMovementScripts();
        }
        
        // Verifica che l'NPC abbia un NavMeshAgent correttamente configurato
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            bool hasIssues = false;
            
            // Verifica le configurazioni problematiche
            if (agent.speed <= 0.1f)
            {
                if (logDiagnostics)
                    Debug.LogWarning("NavMeshAgent ha una velocità troppo bassa: " + agent.speed);
                hasIssues = true;
            }
            
            if (agent.isStopped)
            {
                if (logDiagnostics)
                    Debug.LogWarning("NavMeshAgent è in stato 'isStopped'");
                hasIssues = true;
            }
            
            if (agent.updatePosition == false)
            {
                if (logDiagnostics)
                    Debug.LogWarning("NavMeshAgent non sta aggiornando la posizione (updatePosition = false)");
                hasIssues = true;
            }
            
            Animator animator = GetComponent<Animator>();
            if (animator != null && animator.applyRootMotion)
            {
                if (logDiagnostics)
                    Debug.LogWarning("L'Animator sta usando root motion, che può interferire con NavMeshAgent");
                hasIssues = true;
            }
            
            // Correggi automaticamente le impostazioni problematiche
            if (hasIssues && fixNavMeshAgent)
            {
                FixNavMeshAgentSettings(agent, animator);
            }
        }
        else
        {
            if (logDiagnostics)
                Debug.LogError("NavMeshAgent component non trovato!");
        }
        
        // Verifica che l'NPC sia sulla NavMesh
        if (agent != null)
        {
            if (!agent.isOnNavMesh)
            {
                if (logDiagnostics)
                    Debug.LogError("NPC non si trova sulla NavMesh! Devi posizionarlo in un'area navigabile.");
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
                {
                    if (logDiagnostics)
                        Debug.Log("Trovata posizione valida sulla NavMesh a " + hit.position);
                    
                    if (fixNavMeshAgent)
                    {
                        transform.position = hit.position;
                        if (logDiagnostics)
                            Debug.Log("NPC spostato sulla NavMesh più vicina");
                    }
                }
                else
                {
                    if (logDiagnostics)
                        Debug.LogError("Impossibile trovare una posizione valida sulla NavMesh nelle vicinanze!");
                }
            }
        }
    }
    
    private void FixMultipleMovementScripts()
    {
        // Adattato per utilizzare SimplifiedNPC invece dei vecchi controller
        SimplifiedNPC mainController = GetComponent<SimplifiedNPC>();
        bool hasMainController = mainController != null && mainController.enabled;
        
        // Cerca altri script che potrebbero interferire
        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script == null || script == this || script is SimplifiedNPC)
                continue;
                
            string scriptName = script.GetType().Name;
            
            // Verifica se lo script è legato al movimento dell'NPC
            if (scriptName.Contains("Movement") || 
                scriptName.Contains("Animation") || 
                scriptName.Contains("Controller") ||
                scriptName.Contains("NPC"))
            {
                if (hasMainController && logDiagnostics)
                {
                    Debug.LogWarning($"Script potenzialmente conflittuale trovato: {scriptName}. " +
                                    "Considera di disabilitarlo se SimplifiedNPC è in uso.");
                }
            }
        }
        
        // Verifica che sia presente almeno un controller di movimento
        if (!hasMainController && logDiagnostics)
        {
            Debug.LogWarning("Nessun controller di movimento principale (SimplifiedNPC) trovato.");
        }
    }
    
    private void FixNavMeshAgentSettings(NavMeshAgent agent, Animator animator)
    {
        // Correggi le impostazioni problematiche dell'agent
        if (agent.speed <= 0.1f)
            agent.speed = 2.0f;
        
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
        
        // Disabilita root motion se l'animatore lo usa
        if (animator != null && animator.applyRootMotion)
            animator.applyRootMotion = false;
        
        if (logDiagnostics)
            Debug.Log("Impostazioni NavMeshAgent corrette");
    }
    
    // Per utilizzo da Inspector
    [ContextMenu("Diagnostica e Risoluzione Problemi")]
    public void RunDiagnostics()
    {
        Debug.Log("Avvio diagnostica NPC...");
        Start();
    }
}
