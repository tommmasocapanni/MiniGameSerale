using UnityEngine;

public class ShootableItem : CollectibleItem
{
    [Header("Shooting Properties")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ParticleSystem muzzleFlashEffect;
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float bulletLifetime = 3f;
    [SerializeField] private float recoilForce = 1f; // Adjust this to control recoil strength
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime = 0f;

    [Header("Camera Reference")]
    [SerializeField] private Camera playerCamera;

    protected override void Fire()
    {
        base.Fire();

        // Get the exact center of the screen for shooting direction
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 shootDirection = ray.direction;

        // Create bullet at firePoint position but with camera's direction
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        // Play muzzle flash
        if (muzzleFlashEffect != null)
        {
            muzzleFlashEffect.Stop(true);
            muzzleFlashEffect.Clear();
            muzzleFlashEffect.Play();
        }

        // Add force to bullet using camera direction
        bulletRb.linearVelocity = Vector3.zero;
        bulletRb.AddForce(shootDirection * bulletSpeed, ForceMode.Impulse);

        // Apply recoil to the player
        Rigidbody playerRb = transform.root.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(-shootDirection * recoilForce, ForceMode.Impulse);
        }

        Destroy(bullet, bulletLifetime);
    }

    protected override void HandleInput()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
        
        if (Input.GetMouseButtonUp(0) && playerAnimator != null)
        {
            playerAnimator.SetBool("isFiring", false);
        }
    }

    public override void EquipItem()
    {
        base.EquipItem();
        playerCamera = Camera.main;
        
        // Ensure muzzle flash is stopped when equipping
        if (muzzleFlashEffect != null)
        {
            muzzleFlashEffect.Stop();
            muzzleFlashEffect.Clear();
        }
    }
}
