using UnityEngine;

public class ShieldCollisionHandler : MonoBehaviour
{
    [Header("Shield Stats")]
    [SerializeField] private int maxBlocks = 1; // Chỉ chặn được 1 viên đạn
    private int currentBlocks;
    private int ownerPlayerId = -1;
    private JetsBehaviour ownerJet;
    
    public void Initialize(int playerId, JetsBehaviour jet)
    {
        ownerPlayerId = playerId;
        ownerJet = jet;
        currentBlocks = maxBlocks;
        Debug.Log($"🛡️ Shield initialized for Player {playerId} - can block {maxBlocks} projectile(s)");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🛡️ Shield hit by: {other.name} | Tag: {other.tag}");
        
        // Chặn player bullets
        if (other.CompareTag("Player1Bullet") || other.CompareTag("Player2Bullet"))
        {
            HandleProjectileBlock(other);
        }
        // Chặn turret bullets
        else if (other.name.Contains("TurretBullet"))
        {
            HandleProjectileBlock(other);
        }
        // NOTE: Saber và CanDestroyBlock/CannotDestroyBlock sẽ được ignore qua physics matrix
    }
    
    private void HandleProjectileBlock(Collider2D projectile)
    {
        // Kiểm tra xem đạn có phải từ enemy không
        bool shouldBlock = false;
        
        if (projectile.CompareTag("Player1Bullet") && ownerPlayerId == 2)
        {
            shouldBlock = true;
        }
        else if (projectile.CompareTag("Player2Bullet") && ownerPlayerId == 1)
        {
            shouldBlock = true;
        }
        else if (projectile.name.Contains("TurretBullet"))
        {
            shouldBlock = true; // Shield chặn tất cả turret bullets
        }
        
        if (shouldBlock && currentBlocks > 0)
        {
            // Block projectile
            BlockProjectile(projectile);
        }
    }
    
    private void BlockProjectile(Collider2D projectile)
    {
        currentBlocks--;
        Debug.Log($"🛡️ PROJECTILE BLOCKED! Shield durability: {currentBlocks}/{maxBlocks}");
        
        // Destroy/return projectile
        BasicBulletBehaviour bullet = projectile.GetComponent<BasicBulletBehaviour>();
        if (bullet != null)
        {
            bullet.ReturnToPool();
        }
        else
        {
            // Handle turret bullets
            TurretBulletBehaviour turretBullet = projectile.GetComponent<TurretBulletBehaviour>();
            if (turretBullet != null)
            {
                // Return to turret pool or destroy
                if (TurretBulletPool.Instance != null)
                {
                    TurretBulletPool.Instance.ReturnBullet(projectile.gameObject);
                }
                else
                {
                    Destroy(projectile.gameObject);
                }
            }
        }
        
        // Check if shield is depleted
        if (currentBlocks <= 0)
        {
            DeactivateShield();
        }
    }
    
    private void DeactivateShield()
    {
        Debug.Log("🛡️ SHIELD DEPLETED! No more blocks remaining");
        
        // Disable shield visual
        if (ownerJet != null)
        {
            Transform shield = ownerJet.transform.Find("Shield");
            if (shield != null) 
            {
                shield.gameObject.SetActive(false);
            }
        }
    }
    
    public int GetRemainingBlocks()
    {
        return currentBlocks;
    }
} 