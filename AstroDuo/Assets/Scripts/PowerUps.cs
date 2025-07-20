using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private WeaponType weaponType; 
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float rotationSpeed = 50f; 
    
    void Update()
    {
        // rotate
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        JetsBehaviour jet = other.GetComponent<JetsBehaviour>();
        if (jet != null)
        {
            Debug.Log($"🎁 PowerUp collected by {other.name}: {weaponType}");
            
            IWeapon weapon = CreateWeapon(weaponType);
            if (weapon != null)
            {
                jet.EquipWeapon(weapon);
                
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                Debug.Log($"💀 PowerUp {weaponType} destroyed after pickup!");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"❌ Failed to create weapon for {weaponType}");
            }
        }
    }
    
    private IWeapon CreateWeapon(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.BasicBullet:
                return new BasicWeapon();
            case WeaponType.Laser:
                return new LaserWeapon();
            case WeaponType.Mine:
                return new MineWeapon();
            case WeaponType.Saber:
                return new SaberWeapon();
            case WeaponType.ScatterShot:
                return new ScatterWeapon();
            case WeaponType.Shield:
                return new ShieldWeapon();
            default:
                Debug.LogWarning($"⚠️ Weapon type {type} not implemented yet!");
                return null;
        }
    }
}

public class LaserWeapon : SingleUseWeapon 
{
    public override WeaponType WeaponType => WeaponType.Laser;
    
    protected override void ExecuteFire(Transform firePoint, Vector3 direction)
    {
        Debug.Log("🔴 LASER BEAM FIRED! *intense laser sound*");
        // TODO: Implement actual laser logic later
    }
}

public class MineWeapon : SingleUseWeapon
{
    public override WeaponType WeaponType => WeaponType.Mine;
    
    protected override void ExecuteFire(Transform firePoint, Vector3 direction)
    {
        Debug.Log("💣 MINE DEPLOYED! *strategic placement*");
        // TODO: Implement mine deployment logic later
    }
}

public class SaberWeapon : IWeapon // CHANGE: Không inherit SingleUseWeapon
{
    public WeaponType WeaponType => WeaponType.Saber;
    
    private JetsBehaviour ownerJet;
    private GameObject leftSaber;
    private GameObject rightSaber;
    private float speedBoost = 1.5f;
    private int saberHP = 2;
    private bool isActivated = false; // NEW: Track activation state
    
    public bool CanFire()
    {
        return saberHP > 0; // Chỉ cần check HP
    }
    
    public void Fire(Transform firePoint, Vector3 direction)
    {
        if (!CanFire()) return;
        
        ExecuteFire(firePoint, direction);
        isActivated = true;
        
        // AUTO-SWITCH BACK TO BASIC WEAPON
        if (ownerJet != null)
        {
            // Keep sabers active but switch weapon
            ownerJet.SwitchToBasicWeaponKeepEffects();
        }
    }
    
    public void OnEquipped(JetsBehaviour jet)
    {
        ownerJet = jet;
        
        // Find saber objects
        leftSaber = jet.transform.Find("LeftSaber")?.gameObject;
        rightSaber = jet.transform.Find("RightSaber")?.gameObject;
        
        if (leftSaber == null || rightSaber == null)
        {
            Debug.LogError("❌ Saber objects not found on Jet prefab!");
        }
    }
    
    protected void ExecuteFire(Transform firePoint, Vector3 direction)
    {
        Debug.Log("⚔️ SABER ACTIVATED! *melee mode engaged*");
        
        // ADD DEBUG
        Debug.Log($"🔍 leftSaber: {leftSaber}, rightSaber: {rightSaber}, ownerJet: {ownerJet}");
        
        if (leftSaber != null && rightSaber != null && ownerJet != null)
        {
            saberHP = 2;
            
            // ADD DEBUG BEFORE
            Debug.Log($"🔍 Before SetActive - LeftSaber active: {leftSaber.activeInHierarchy}, RightSaber active: {rightSaber.activeInHierarchy}");
            
            leftSaber.SetActive(true);
            rightSaber.SetActive(true);
            
            // ADD DEBUG AFTER
            Debug.Log($"🔍 After SetActive - LeftSaber active: {leftSaber.activeInHierarchy}, RightSaber active: {rightSaber.activeInHierarchy}");
            
            SetupCollisionForwarding(leftSaber);
            SetupCollisionForwarding(rightSaber);
            ownerJet.ApplySpeedBoost(speedBoost);
            
            // PLAY SABER SOUND
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySaberSound();
            }
            
            Debug.Log($"⚔️ Saber activated with {speedBoost}x speed boost! HP: {saberHP}");
        }
        else
        {
            Debug.LogError($"❌ Cannot activate saber! leftSaber={leftSaber}, rightSaber={rightSaber}, ownerJet={ownerJet}");
        }
    }
    
    private void SetupCollisionForwarding(GameObject saber)
    {
        // Add simple collision forwarder nếu chưa có
        CollisionForwarder forwarder = saber.GetComponent<CollisionForwarder>();
        if (forwarder == null)
        {
            forwarder = saber.AddComponent<CollisionForwarder>();
        }
        
        // Setup callback
        forwarder.OnTriggerEnterCallback = (other) => HandleSaberCollision(saber, other);
    }
    
    private void HandleSaberCollision(GameObject saber, Collider2D other)
    {
        Debug.Log($"⚔️ Saber hit: {other.name} | Tag: {other.tag}");
        
        // ALLOW: CanDestroyBlock
        if (other.CompareTag("CanDestroyBlock"))
        {
            HandleBlockDestruction(other);
        }
        // ALLOW: Enemy Jets
        else if (other.CompareTag("Jet"))
        {
            HandleJetCollision(other);
        }
        // IGNORE: CannotDestroyBlock (do nothing)
        else if (other.CompareTag("CannotDestroyBlock"))
        {
            Debug.Log("⚔️ Saber hit indestructible block - ignored");
            return;
        }
        // IGNORE: Shield (penetrate)
        else if (other.name.Contains("Shield"))
        {
            Debug.Log("⚔️ Saber penetrated shield!");
            return;
        }
        // IGNORE: Own bullets
        string ownBulletTag = $"Player{ownerJet.playerId}Bullet";
        if (other.CompareTag(ownBulletTag))
        {
            Debug.Log("⚔️ Saber ignored own bullet");
            return;
        }
    }
    
    private void HandleBlockDestruction(Collider2D blockCollider)
    {
        UnityEngine.Tilemaps.Tilemap tilemap = blockCollider.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        if (tilemap != null)
        {
            Vector3 saberPosition = leftSaber.transform.position; // Use any saber position
            
            if (DestroyTileAtPosition(tilemap, saberPosition))
            {
                TakeSaberDamage(1);
                Debug.Log($"⚔️ SABER DESTROYED BLOCK! HP: {saberHP}/2");
            }
        }
    }
    
    private bool DestroyTileAtPosition(UnityEngine.Tilemaps.Tilemap tilemap, Vector3 worldPosition)
    {
        Vector3[] testPositions = {
            worldPosition,
            worldPosition + Vector3.up * 0.2f,
            worldPosition + Vector3.down * 0.2f,
            worldPosition + Vector3.left * 0.2f,
            worldPosition + Vector3.right * 0.2f
        };
        
        foreach (Vector3 testPos in testPositions)
        {
            Vector3Int tilePosition = tilemap.WorldToCell(testPos);
            UnityEngine.Tilemaps.TileBase tileAtPosition = tilemap.GetTile(tilePosition);
            
            if (tileAtPosition != null)
            {
                tilemap.SetTile(tilePosition, null);
                Debug.Log($"✅ Saber destroyed tile at {tilePosition}");
                return true;
            }
        }
        return false;
    }
    
    private void HandleJetCollision(Collider2D jetCollider)
    {
        JetsBehaviour hitJet = jetCollider.GetComponent<JetsBehaviour>();
        if (hitJet != null && hitJet.playerId != ownerJet.playerId)
        {
            // Instant kill (-2 HP)
            TakeSaberDamage(2);
            
            // Award point
            if (GameManager.Instance != null)
            {
                if (ownerJet.playerId == 1)
                {
                    GameManager.Instance.AddPlayer1Score();
                }
                else if (ownerJet.playerId == 2)
                {
                    GameManager.Instance.AddPlayer2Score();
                }
                
                Debug.Log($"⚔️ SABER KILL! Player {ownerJet.playerId} destroyed Player {hitJet.playerId} (penetrated shield!)");
                UnityEngine.SceneManagement.SceneManager.LoadScene(GameConst.SCORE_SCENE);
            }
        }
    }
    
    private void TakeSaberDamage(int damage)
    {
        saberHP -= damage;
        Debug.Log($"⚔️ Saber took {damage} damage! HP: {saberHP}/2");
        
        if (saberHP <= 0)
        {
            DeactivateSaber();
        }
    }
    
    private void DeactivateSaber()
    {
        Debug.Log("⚔️ SABER DESTROYED! HP reached 0");
        
        if (leftSaber != null) leftSaber.SetActive(false);
        if (rightSaber != null) rightSaber.SetActive(false);
        
        if (ownerJet != null)
        {
            ownerJet.ResetSpeedBoost();
        }
        
        // STOP SABER SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopSaberSound();
        }
    }
    
    public void OnUnequipped(JetsBehaviour jet)
    {
        DeactivateSaber();
    }
}

public class ScatterWeapon : IWeapon // CHANGE: Không inherit SingleUseWeapon
{
    public WeaponType WeaponType => WeaponType.ScatterShot;
    
    private JetsBehaviour ownerJet;
    private bool isUsed = false;
    private int scatterBulletCount = 12; // 12 viên đạn
    private float scatterBulletSpeed = 12f;
    private GameObject scatterBulletPrefab; // ADD THIS LINE
    
    public bool CanFire()
    {
        return !isUsed; // Chỉ bắn được 1 lần
    }
    
    public void Fire(Transform firePoint, Vector3 direction)
    {
        if (!CanFire()) return;
        
        ExecuteFire(firePoint, direction);
        isUsed = true;
        
        // AUTO-SWITCH BACK TO BASIC WEAPON
        if (ownerJet != null)
        {
            ownerJet.SwitchToBasicWeaponKeepEffects();
        }
    }
    
    public void OnEquipped(JetsBehaviour jet)
    {
        ownerJet = jet;
        
        // Get prefab reference từ Jet
        scatterBulletPrefab = jet.GetScatterBulletPrefab();
        
        if (scatterBulletPrefab == null)
        {
            Debug.LogError("❌ ScatterBullet prefab not assigned in JetsBehaviour!");
        }
        else
        {
            Debug.Log("✅ ScatterBullet prefab loaded from Jet!");
        }
        
        Debug.Log("💥 ScatterShot equipped! Single use available.");
    }
    
    public void OnUnequipped(JetsBehaviour jet)
    {
        Debug.Log("📤 ScatterShot unequipped!");
    }
    
    private void ExecuteFire(Transform firePoint, Vector3 direction)
    {
        Debug.Log("💥 SCATTER SHOT FIRED! *multiple projectiles*");
        
        if (ownerJet == null) return;
        
        // Bắn 12 viên đạn theo hình tròn (360° / 12 = 30° mỗi viên)
        float angleStep = 360f / scatterBulletCount;
        
        for (int i = 0; i < scatterBulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 scatterDirection = GetDirectionFromAngle(angle);
            FireScatterBullet(firePoint.position, scatterDirection);
        }
        
        Debug.Log($"💥 Fired {scatterBulletCount} scatter bullets in all directions!");
        
        // PLAY SCATTER SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayScatterShotSound();
        }
    }
    
    private Vector3 GetDirectionFromAngle(float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians), 0f);
    }
    
    private void FireScatterBullet(Vector3 position, Vector3 direction)
    {
        GameObject scatterBullet = CreateScatterBullet();
        if (scatterBullet == null) return;
        
        // SET PROPER TAG based on owner
        scatterBullet.tag = $"Player{ownerJet.playerId}ScatterBullet";
        
        float spawnOffset = 0.5f;
        Vector3 spawnPosition = position + (direction * spawnOffset);
        
        scatterBullet.transform.position = spawnPosition;
        scatterBullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        
        ScatterBulletBehaviour bulletBehaviour = scatterBullet.GetComponent<ScatterBulletBehaviour>();
        if (bulletBehaviour != null)
        {
            bulletBehaviour.SetOwner(ownerJet.playerId, ownerJet.gameObject.name, ownerJet.gameObject);
        }
        
        Rigidbody2D rb = scatterBullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * scatterBulletSpeed;
        }
    }
    
    private GameObject CreateScatterBullet()
    {
        if (scatterBulletPrefab != null)
        {
            GameObject bullet = Object.Instantiate(scatterBulletPrefab);
            
            // REMOVE THIS LINE - để scale mặc định của prefab
            // bullet.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            
            return bullet;
        }
        
        Debug.LogError("❌ ScatterBullet prefab is null!");
        return null;
    }
    
    private Sprite CreateScatterSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.yellow); // Màu vàng cho scatter bullets
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
    }
}

public class ShieldWeapon : IWeapon // CHANGE: Không inherit SingleUseWeapon
{
    public WeaponType WeaponType => WeaponType.Shield;
    
    private JetsBehaviour ownerJet;
    private GameObject shield;
    private int shieldBlocks = 1; // Chỉ chặn được 1 projectile
    private bool isActivated = false; // NEW: Track activation state
    
    public bool CanFire()
    {
        return shieldBlocks > 0; // Có thể fire khi còn blocks
    }
    
    public void Fire(Transform firePoint, Vector3 direction)
    {
        if (!CanFire()) return;
        
        ExecuteFire(firePoint, direction);
        isActivated = true;
        
        // AUTO-SWITCH BACK TO BASIC WEAPON
        if (ownerJet != null)
        {
            // Keep shield active but switch weapon
            ownerJet.SwitchToBasicWeaponKeepEffects();
        }
    }
    
    public void OnEquipped(JetsBehaviour jet)
    {
        ownerJet = jet;
        
        shield = jet.transform.Find("Shield")?.gameObject;
        
        // ADD DEBUG LOG
        if (shield == null)
        {
            Debug.LogError($"❌ Shield object not found on Jet prefab! Available children:");
            for (int i = 0; i < jet.transform.childCount; i++)
            {
                Debug.Log($"   Child {i}: {jet.transform.GetChild(i).name}");
            }
        }
        else
        {
            Debug.Log($"✅ Shield found: {shield.name}");
        }
    }
    
    protected void ExecuteFire(Transform firePoint, Vector3 direction)
    {
        Debug.Log("🛡️ SHIELD ACTIVATED! *defensive mode engaged*");
        
        // ADD DEBUG
        Debug.Log($"🔍 shield: {shield}, ownerJet: {ownerJet}");
        
        if (shield != null && ownerJet != null)
        {
            shieldBlocks = 1;
            
            // ADD DEBUG BEFORE
            Debug.Log($"🔍 Before SetActive - Shield active: {shield.activeInHierarchy}");
            
            shield.SetActive(true);
            
            // ADD DEBUG AFTER
            Debug.Log($"🔍 After SetActive - Shield active: {shield.activeInHierarchy}");
            
            SetupCollisionForwarding();
            
            // PLAY SHIELD SOUND
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShieldSound();
            }
            
            Debug.Log("🛡️ Shield deployed - can block 1 projectile!");
        }
        else
        {
            Debug.LogError($"❌ Cannot activate shield! shield={shield}, ownerJet={ownerJet}");
        }
    }
    
    private void SetupCollisionForwarding()
    {
        CollisionForwarder forwarder = shield.GetComponent<CollisionForwarder>();
        if (forwarder == null)
        {
            forwarder = shield.AddComponent<CollisionForwarder>();
        }
        
        forwarder.OnTriggerEnterCallback = (other) => HandleShieldCollision(other);
    }
    
    private void HandleShieldCollision(Collider2D other)
    {
        Debug.Log($"🛡️ Shield hit by: {other.name} | Tag: {other.tag}");
        
        // IGNORE: Blocks completely
        if (other.CompareTag("CanDestroyBlock") || other.CompareTag("CannotDestroyBlock"))
        {
            Debug.Log($"🛡️ Shield ignored block: {other.tag}");
            return;
        }
        
        // BLOCK: Enemy bullets
        if (other.CompareTag("Player1Bullet") && ownerJet.playerId == 2)
        {
            BlockProjectile(other);
        }
        else if (other.CompareTag("Player2Bullet") && ownerJet.playerId == 1)
        {
            BlockProjectile(other);
        }
        else if (other.name.Contains("TurretBullet"))
        {
            BlockProjectile(other);
        }
        // HANDLE: ScatterBullet by tag
        else if (other.CompareTag("Player1ScatterBullet") && ownerJet.playerId == 2)
        {
            Debug.Log($"🛡️ Shield blocking Player 1 scatter bullet");
            BlockScatterBullet(other);
        }
        else if (other.CompareTag("Player2ScatterBullet") && ownerJet.playerId == 1)
        {
            Debug.Log($"🛡️ Shield blocking Player 2 scatter bullet");
            BlockScatterBullet(other);
        }
        // IGNORE: Own scatter bullets
        else if (other.CompareTag($"Player{ownerJet.playerId}ScatterBullet"))
        {
            Debug.Log($"🛡️ Shield ignored own scatter bullet");
            return;
        }
        // IGNORE: Everything else
        else
        {
            Debug.Log($"🛡️ Shield ignored: {other.tag}");
            return;
        }
    }
    
    private void BlockProjectile(Collider2D projectile)
    {
        shieldBlocks--;
        Debug.Log($"🛡️ PROJECTILE BLOCKED! Shield blocks remaining: {shieldBlocks}");
        
        // Return projectile to pool
        BasicBulletBehaviour bullet = projectile.GetComponent<BasicBulletBehaviour>();
        if (bullet != null)
        {
            bullet.ReturnToPool();
        }
        else
        {
            TurretBulletBehaviour turretBullet = projectile.GetComponent<TurretBulletBehaviour>();
            if (turretBullet != null)
            {
                if (TurretBulletPool.Instance != null)
                {
                    TurretBulletPool.Instance.ReturnBullet(projectile.gameObject);
                }
                else
                {
                    UnityEngine.Object.Destroy(projectile.gameObject);
                }
            }
        }
        
        // Check if shield depleted
        if (shieldBlocks <= 0)
        {
            DeactivateShield();
        }
    }
    
    // NEW: Method to block scatter bullets
    private void BlockScatterBullet(Collider2D scatterBullet)
    {
        shieldBlocks--;
        Debug.Log($"🛡️ SCATTER BULLET BLOCKED! Shield blocks remaining: {shieldBlocks}");
        
        // Destroy scatter bullet
        ScatterBulletBehaviour bulletBehaviour = scatterBullet.GetComponent<ScatterBulletBehaviour>();
        if (bulletBehaviour != null)
        {
            bulletBehaviour.DestroyBullet(); // Call public method
        }
        else
        {
            UnityEngine.Object.Destroy(scatterBullet.gameObject);
        }
        
        // Check if shield depleted
        if (shieldBlocks <= 0)
        {
            DeactivateShield();
        }
    }
    
    private void DeactivateShield()
    {
        Debug.Log("🛡️ SHIELD DEPLETED!");
        if (shield != null) shield.SetActive(false);
        
        // STOP SHIELD SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopShieldSound();
        }
    }
    
    public void OnUnequipped(JetsBehaviour jet)
    {
        DeactivateShield();
    }
}