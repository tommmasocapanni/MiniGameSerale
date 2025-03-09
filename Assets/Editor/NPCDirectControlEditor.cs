using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CustomEditor(typeof(SimplifiedNPC))]
public class NPCDirectControlEditor : Editor
{
    private bool showDirectControls = false;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SimplifiedNPC npc = (SimplifiedNPC)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Controlli Diretti", EditorStyles.boldLabel);
        
        if (Application.isPlaying && npc.enabled)
        {
            if (GUILayout.Button("Applica Impostazioni"))
            {
                npc.ApplicaImpostazioniMovimento();
            }
            
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            
            if (agent != null)
            {
                showDirectControls = EditorGUILayout.Foldout(showDirectControls, "Controllo Diretto NavMeshAgent");
                
                if (showDirectControls)
                {
                    EditorGUI.BeginChangeCheck();
                    float speed = EditorGUILayout.Slider("Velocità Agente", agent.speed, 0.1f, 10f);
                    float angularSpeed = EditorGUILayout.Slider("Velocità Rotazione", agent.angularSpeed, 10f, 500f);
                    float acceleration = EditorGUILayout.Slider("Accelerazione", agent.acceleration, 1f, 20f);
                    float stoppingDistance = EditorGUILayout.Slider("Distanza Arresto", agent.stoppingDistance, 0.1f, 2f);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(agent, "Modifica NavMeshAgent");
                        
                        agent.speed = speed;
                        agent.angularSpeed = angularSpeed;
                        agent.acceleration = acceleration;
                        agent.stoppingDistance = stoppingDistance;
                        
                        // Aggiorna anche i valori nel SimplifiedNPC
                        npc.velocitaMovimento = speed;
                        npc.distanzaArresto = stoppingDistance;
                        
                        EditorUtility.SetDirty(npc);
                    }
                }
            }
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Muovi NPC"))
            {
                npc.ForzaMovimento();
            }
            
            if (GUILayout.Button("Ferma NPC"))
            {
                npc.ForzaArresto();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Controlli disponibili solo durante il play mode", MessageType.Info);
        }
    }
}
