using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AI;
#endif

public class NavMeshSetup : MonoBehaviour
{
    [Header("NavMesh Settings")]
    public LayerMask groundLayers = 1; // Default layer
    public bool autoAddGroundObjects = true;
    public float navMeshBakeDelay = 0.5f;
    
    void Start()
    {
        // Trova tutti gli oggetti con tag "Ground" e verifica se sono configurati per NavMesh
        if (autoAddGroundObjects)
        {
            GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("Ground");
            
            if (groundObjects.Length == 0)
            {
                Debug.LogWarning("Nessun oggetto con tag 'Ground' trovato nella scena!");
            }
            else
            {
                Debug.Log("Trovati " + groundObjects.Length + " oggetti con tag 'Ground'");
                
                foreach (GameObject ground in groundObjects)
                {
                    // Verifica che abbiano collider (necessario per NavMesh)
                    Collider collider = ground.GetComponent<Collider>();
                    if (collider == null)
                    {
                        Debug.LogWarning("L'oggetto Ground '" + ground.name + "' non ha un collider! La NavMesh non funzionerà su questo oggetto.");
                    }
                    
                    // Verifica che siano sul layer corretto
                    if (ground.layer != LayerMask.NameToLayer("Ground"))
                    {
                        Debug.LogWarning("L'oggetto '" + ground.name + "' ha il tag 'Ground' ma non è nel layer 'Ground'!");
                    }
                }
            }
        }
        
        #if UNITY_EDITOR
        // Questo codice si eseguirà solo nell'editor, non in build
        Invoke("LogNavMeshAreaInfo", navMeshBakeDelay);
        #endif
    }
    
    #if UNITY_EDITOR
    private void LogNavMeshAreaInfo()
    {
        Debug.Log("Info NavMesh: Assicurati che il layer 'Ground' sia incluso nelle impostazioni di bake della NavMesh.");
        Debug.Log("Per configurare la NavMesh: Window > AI > Navigation > Bake > Advanced > Layer Mask");
    }
    
    [ContextMenu("Verifica e Genera NavMesh")]
    private void RebakeNavMesh()
    {
        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("Ground");
        bool hasStaticObjects = false;
        
        foreach (GameObject ground in groundObjects)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(ground);
            if ((flags & StaticEditorFlags.NavigationStatic) != StaticEditorFlags.NavigationStatic)
            {
                GameObjectUtility.SetStaticEditorFlags(ground, 
                    flags | StaticEditorFlags.NavigationStatic);
                Debug.Log("Impostato NavigationStatic su " + ground.name);
            }
            else
            {
                hasStaticObjects = true;
            }
        }
        
        if (hasStaticObjects)
        {
            NavMeshBuilder.ClearAllNavMeshes();
            NavMeshBuilder.BuildNavMesh();
            Debug.Log("NavMesh rigenerata con successo!");
        }
        else
        {
            Debug.LogWarning("Nessun oggetto NavigationStatic trovato. La NavMesh potrebbe essere vuota.");
        }
    }
    #endif
}
