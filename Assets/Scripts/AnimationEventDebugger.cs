using UnityEngine;

public class AnimationEventDebugger : MonoBehaviour
{
    public bool logStateChanges = true;
    public bool logParameters = true;
    public float logInterval = 2.0f;
    
    private Animator animator;
    private float lastLogTime;
    private int lastLayerIndex = -1;
    private AnimatorStateInfo lastState;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
            enabled = false;
            return;
        }
        
        lastLogTime = Time.time;
        if (animator.layerCount > 0)
        {
            lastState = animator.GetCurrentAnimatorStateInfo(0);
            lastLayerIndex = 0;
        }
    }
    
    private void Update()
    {
        if (animator == null) return;
        
        // Log state changes
        if (logStateChanges && animator.layerCount > 0)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (currentState.fullPathHash != lastState.fullPathHash)
            {
                Debug.Log($"[{gameObject.name}] Animation state changed to: {currentState.shortNameHash}");
                lastState = currentState;
            }
        }
        
        // Log parameters periodically
        if (logParameters && Time.time > lastLogTime + logInterval)
        {
            LogAnimatorParameters();
            lastLogTime = Time.time;
        }
    }
    
    private void LogAnimatorParameters()
    {
        string paramLog = $"[{gameObject.name}] Animator Parameters:\n";
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    paramLog += $"  {param.name} (float): {animator.GetFloat(param.name)}\n";
                    break;
                case AnimatorControllerParameterType.Int:
                    paramLog += $"  {param.name} (int): {animator.GetInteger(param.name)}\n";
                    break;
                case AnimatorControllerParameterType.Bool:
                    paramLog += $"  {param.name} (bool): {animator.GetBool(param.name)}\n";
                    break;
                case AnimatorControllerParameterType.Trigger:
                    paramLog += $"  {param.name} (trigger)\n";
                    break;
            }
        }
        
        Debug.Log(paramLog);
    }
    
    [ContextMenu("Log Animator Properties")]
    public void ForceLogAnimatorProperties()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found");
                return;
            }
        }
        
        Debug.Log($"[{gameObject.name}] Animator Properties:");
        Debug.Log($"  Is enabled: {animator.enabled}");
        Debug.Log($"  Culling mode: {animator.cullingMode}");
        Debug.Log($"  Update mode: {animator.updateMode}");
        Debug.Log($"  Apply root motion: {animator.applyRootMotion}");
        
        LogAnimatorParameters();
    }
}
