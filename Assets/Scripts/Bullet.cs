using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private ParticleSystem bulletTrailEffect;
    [SerializeField] private ParticleSystem explosionEffect;
    private bool hasCollided = false;
    
    private void Start()
    {
        // Start trail effect immediately
        if (bulletTrailEffect != null)
        {
            bulletTrailEffect.Play();
            Debug.Log("Trail effect started");
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return; // Prevent multiple collisions
        hasCollided = true;
        
        Debug.Log("Bullet collided with: " + collision.gameObject.name);

        // Create and play explosion effect
        if (explosionEffect != null)
        {
            ParticleSystem explosion = Instantiate(explosionEffect, 
                collision.contacts[0].point, 
                Quaternion.LookRotation(collision.contacts[0].normal));
                
            explosion.Play();
            Debug.Log("Explosion effect instantiated and played");
            
            // Destroy the explosion object after its duration
            float duration = explosion.main.duration;
            Destroy(explosion.gameObject, duration);
        }

        // Stop trail and destroy bullet
        if (bulletTrailEffect != null)
        {
            bulletTrailEffect.Stop();
        }

        // Delay destruction slightly to ensure effects are visible
        Destroy(gameObject, 0.1f);
    }
}
