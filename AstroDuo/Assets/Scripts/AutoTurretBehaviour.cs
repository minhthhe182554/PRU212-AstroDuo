using UnityEngine;
using System.Collections;

public class AutoTurretBehaviour : MonoBehaviour
{
    [Header("Turret Settings")]
    [SerializeField] private float rotationSpeed = 60f; // Degrees per second
    [SerializeField] private float fireRate = 2f; // Bullets per second
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private Transform firePoint;
    
    [Header("Visual")]
    [SerializeField] private Transform turretBarrel; // Optional: separate barrel to rotate
    
    private float lastFireTime;
    private float fireCooldown;
    
    void Start()
    {
        fireCooldown = 1f / fireRate;
        
        // Find or create fire point
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
            if (firePoint == null)
            {
                // Create fire point at turret position + offset
                GameObject firePointObj = new GameObject("FirePoint");
                firePoint = firePointObj.transform;
                firePoint.SetParent(transform);
                firePoint.localPosition = new Vector3(0, 0.5f, 0); // Above turret center
            }
        }
        
        // If no separate barrel, use the turret itself for rotation
        if (turretBarrel == null)
        {
            turretBarrel = transform;
        }
        
        Debug.Log($"🎯 Auto Turret initialized! Fire rate: {fireRate} bullets/sec, Rotation: {rotationSpeed}°/sec");
    }

    void Update()
    {
        RotateTurret();
        TryFire();
    }
    
    private void RotateTurret()
    {
        // Continuous rotation
        float rotationAmount = rotationSpeed * Time.deltaTime;
        turretBarrel.Rotate(0, 0, rotationAmount);
    }
    
    private void TryFire()
    {
        if (Time.time - lastFireTime >= fireCooldown)
        {
            FireBullet();
            lastFireTime = Time.time;
        }
    }
    
    private void FireBullet()
    {
        if (TurretBulletPool.Instance == null)
        {
            Debug.LogError("❌ TurretBulletPool not found!");
            return;
        }
        
        GameObject bullet = TurretBulletPool.Instance.GetTurretBullet();
        if (bullet == null)
        {
            Debug.LogWarning("⚠️ Could not get turret bullet from pool!");
            return;
        }
        
        // Setup bullet position and movement
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        
        // Fire bullet in the direction the turret is facing
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 fireDirection = firePoint.up; // or transform.up if firePoint follows turret rotation
            rb.linearVelocity = fireDirection * bulletSpeed;
        }
        
        Debug.Log($"🎯 [TURRET FIRED] Bullet speed: {bulletSpeed}, Direction: {firePoint.up}");
    }
    
    // Optional: Method to temporarily stop firing (for power-ups, etc.)
    public void SetFiringEnabled(bool enabled)
    {
        this.enabled = enabled;
    }
    
    // Optional: Method to change fire rate dynamically
    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Max(0.1f, newFireRate); // Minimum 0.1 bullets per second
        fireCooldown = 1f / fireRate;
        Debug.Log($"🎯 Turret fire rate changed to: {fireRate} bullets/sec");
    }
    
    // Optional: Method to change rotation speed
    public void SetRotationSpeed(float newRotationSpeed)
    {
        rotationSpeed = newRotationSpeed;
        Debug.Log($"🎯 Turret rotation speed changed to: {rotationSpeed}°/sec");
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && firePoint != null)
        {
            // Draw fire direction
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, firePoint.up * 2f);
            
            // Draw turret center
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
