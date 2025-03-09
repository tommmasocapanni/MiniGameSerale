using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebugger : MonoBehaviour
{
    public bool showNavMeshPath = true;
    public Color pathColor = Color.green;
    public float sphereRadius = 0.2f;
    
    private NavMeshAgent agent;
    private LineRenderer lineRenderer;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Aggiungi un LineRenderer per visualizzare il percorso
        if (showNavMeshPath && agent != null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = pathColor;
            lineRenderer.endColor = pathColor;
        }
        
        // Verifica se il NavMeshAgent è su una NavMesh valida
        if (agent != null)
        {
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
            {
                Debug.LogError("Il GameObject " + gameObject.name + " non è posizionato su una NavMesh valida!");
            }
            else
            {
                Debug.Log("NavMesh valida trovata sotto " + gameObject.name);
            }
        }
    }
    
    void Update()
    {
        // Se abbiamo un agente e un line renderer, mostra il percorso
        if (agent != null && lineRenderer != null && showNavMeshPath)
        {
            if (agent.hasPath)
            {
                DrawPath(agent.path);
            }
            else
            {
                lineRenderer.positionCount = 0;
            }
        }
    }
    
    void DrawPath(NavMeshPath path)
    {
        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPositions(path.corners);
    }
    
    void OnDrawGizmos()
    {
        // Visualizza posizione corrente
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, sphereRadius);
        
        // Visualizza la destinazione
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(agent.destination, sphereRadius);
        }
    }
}
