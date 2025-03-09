using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CustomEditor(typeof(NavMeshAgent))]
public class NavMeshAgentLockEditor : Editor
{
    private NavMeshAgent agent;
    private SerializedProperty speedProperty;
    private SerializedProperty angularSpeedProperty;
    private SerializedProperty accelerationProperty;
    private SerializedProperty stoppingDistanceProperty;
    private SerializedProperty radiusProperty;
    
    private float originalSpeed;
    private float originalAngularSpeed;
    private float originalAcceleration;
    private float originalStoppingDistance;
    private float originalRadius;
    
    private bool isLocked = false;
    
    private void OnEnable()
    {
        agent = (NavMeshAgent)target;
        
        // Ottieni riferimenti alle proprietà serializzate
        speedProperty = serializedObject.FindProperty("m_Speed");
        angularSpeedProperty = serializedObject.FindProperty("m_AngularSpeed");
        accelerationProperty = serializedObject.FindProperty("m_Acceleration");
        stoppingDistanceProperty = serializedObject.FindProperty("m_StoppingDistance");
        radiusProperty = serializedObject.FindProperty("m_Radius");
        
        // Memorizza i valori originali
        if (agent != null)
        {
            originalSpeed = agent.speed;
            originalAngularSpeed = agent.angularSpeed;
            originalAcceleration = agent.acceleration;
            originalStoppingDistance = agent.stoppingDistance;
            originalRadius = agent.radius;
        }
        
        // Carica stato di lock dalle EditorPrefs
        string agentId = agent.gameObject.name + "_" + agent.gameObject.GetInstanceID();
        isLocked = EditorPrefs.GetBool("NavMeshAgent_Locked_" + agentId, false);
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Aggiungi un pulsante per bloccare/sbloccare i valori
        GUIStyle lockButtonStyle = new GUIStyle(GUI.skin.button);
        lockButtonStyle.normal.textColor = isLocked ? Color.green : Color.white;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Protezione Valori NavMeshAgent", EditorStyles.boldLabel);
        if (GUILayout.Button(isLocked ? "🔒 Bloccato" : "🔓 Sbloccato", lockButtonStyle, GUILayout.Width(100)))
        {
            isLocked = !isLocked;
            string agentId = agent.gameObject.name + "_" + agent.gameObject.GetInstanceID();
            EditorPrefs.SetBool("NavMeshAgent_Locked_" + agentId, isLocked);
            
            if (isLocked)
            {
                // Memorizza i valori attuali come originali
                originalSpeed = agent.speed;
                originalAngularSpeed = agent.angularSpeed;
                originalAcceleration = agent.acceleration;
                originalStoppingDistance = agent.stoppingDistance;
                originalRadius = agent.radius;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Se è in modalità play e bloccato, visualizza i valori originali e permetti
        // di ripristinarli
        if (EditorApplication.isPlaying && isLocked)
        {
            EditorGUILayout.HelpBox("I valori NavMeshAgent sono bloccati. I valori mostrati sono quelli originali, " +
                                    "anche se altri script li hanno modificati in runtime.", MessageType.Info);
            
            if (GUILayout.Button("Ripristina Valori Originali"))
            {
                agent.speed = originalSpeed;
                agent.angularSpeed = originalAngularSpeed;
                agent.acceleration = originalAcceleration;
                agent.stoppingDistance = originalStoppingDistance;
                agent.radius = originalRadius;
            }
            
            // Visualizza i valori attuali vs originali
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginDisabledGroup(true);
            
            EditorGUILayout.LabelField("Valori Attuali vs Originali", EditorStyles.boldLabel);
            EditorGUILayout.FloatField("Speed (Attuale)", agent.speed);
            EditorGUILayout.FloatField("Speed (Originale)", originalSpeed);
            
            EditorGUILayout.FloatField("Angular Speed (Attuale)", agent.angularSpeed);
            EditorGUILayout.FloatField("Angular Speed (Originale)", originalAngularSpeed);
            
            EditorGUILayout.FloatField("Acceleration (Attuale)", agent.acceleration);
            EditorGUILayout.FloatField("Acceleration (Originale)", originalAcceleration);
            
            EditorGUILayout.FloatField("Stopping Distance (Attuale)", agent.stoppingDistance);
            EditorGUILayout.FloatField("Stopping Distance (Originale)", originalStoppingDistance);
            
            EditorGUILayout.FloatField("Radius (Attuale)", agent.radius);
            EditorGUILayout.FloatField("Radius (Originale)", originalRadius);
            
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }
        
        // Mostra il resto dell'inspector standard
        DrawPropertiesExcluding(serializedObject, new string[] { "m_Script" });
        
        serializedObject.ApplyModifiedProperties();
    }
}
