using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] footstepSounds;
    public AudioClip[] idleSounds;
    public AudioClip[] interactionSounds;
    
    [Header("Settings")]
    public float minTimeBetweenIdleSounds = 10f;
    public float maxTimeBetweenIdleSounds = 30f;
    public float footstepRate = 0.5f;
    
    private AudioSource audioSource;
    private float idleSoundTimer;
    private float footstepTimer;
    private Animator animator;
    private bool wasWalking;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        
        // Configura l'AudioSource
        if (audioSource != null)
        {
            audioSource.spatialBlend = 1.0f;  // Audio 3D
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 2.0f;
            audioSource.maxDistance = 15.0f;
        }
        
        ResetIdleTimer();
    }
    
    private void Update()
    {
        if (animator == null || audioSource == null) return;
        
        bool isWalking = animator.GetBool("IsWalking");
        
        // Gestione suoni di passo
        if (isWalking)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayRandomFootstep();
                footstepTimer = footstepRate;
            }
            
            // Resetta il timer per i suoni idle quando inizia a camminare
            if (!wasWalking)
            {
                ResetIdleTimer();
            }
        }
        // Gestione suoni idle
        else
        {
            idleSoundTimer -= Time.deltaTime;
            if (idleSoundTimer <= 0f)
            {
                PlayRandomIdleSound();
                ResetIdleTimer();
            }
        }
        
        wasWalking = isWalking;
    }
    
    public void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0 || audioSource == null) return;
        
        int index = Random.Range(0, footstepSounds.Length);
        audioSource.clip = footstepSounds[index];
        audioSource.volume = Random.Range(0.8f, 1.0f);
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play();
    }
    
    public void PlayRandomIdleSound()
    {
        if (idleSounds.Length == 0 || audioSource == null) return;
        
        int index = Random.Range(0, idleSounds.Length);
        audioSource.clip = idleSounds[index];
        audioSource.volume = Random.Range(0.8f, 1.0f);
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.Play();
    }
    
    public void PlayInteractionSound(int index = -1)
    {
        if (interactionSounds.Length == 0 || audioSource == null) return;
        
        if (index < 0 || index >= interactionSounds.Length)
            index = Random.Range(0, interactionSounds.Length);
        
        audioSource.clip = interactionSounds[index];
        audioSource.volume = 1.0f;
        audioSource.pitch = 1.0f;
        audioSource.Play();
    }
    
    private void ResetIdleTimer()
    {
        idleSoundTimer = Random.Range(minTimeBetweenIdleSounds, maxTimeBetweenIdleSounds);
    }
}
