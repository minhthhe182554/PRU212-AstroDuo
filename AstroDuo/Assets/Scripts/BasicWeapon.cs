using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWeapon : IWeapon
{
    public WeaponType WeaponType => WeaponType.BasicBullet;
    
    private float bulletSpeed = 15f;
    private float fireDelay = 0.08f;
    
    private float lastFireTime;
    
    // Track owner info
    private int ownerId = -1;
    private string ownerName = "";
    private GameObject ownerGameObject;
    
    public BasicWeapon()
    {
        lastFireTime = 0f;
    }
    
    public bool CanFire()
    {
        // Check if enough time has passed since last fire AND if bullets are available in pool
        bool timingOK = Time.time - lastFireTime >= fireDelay;
        bool bulletsAvailable = GetAvailableBullets() > 0;
        
        return timingOK && bulletsAvailable;
    }
    
    public void Fire(Transform firePoint, Vector3 direction)
    {
        if (!CanFire()) return;
        
        // Get bullet from player-specific pool
        if (BulletPool.Instance == null) return;
        
        GameObject bullet = BulletPool.Instance.GetBasicBullet(ownerId);
        if (bullet == null) 
        {
            Debug.LogWarning($"⚠️ Player {ownerId} cannot get bullet - pool exhausted!");
            return;
        }
        
        // IMMEDIATELY set bullet owner BEFORE setting position/rotation
        BasicBulletBehaviour bulletBehaviour = bullet.GetComponent<BasicBulletBehaviour>();
        if (bulletBehaviour != null)
        {
            bulletBehaviour.SetOwner(ownerId, ownerName, ownerGameObject);
        }
        
        // Setup bullet position and movement AFTER owner is set
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        
        // Fire bullet
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
        
        // PLAY SOUND
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBasicBulletSound();
        }
        
        // Update fire timing
        lastFireTime = Time.time;
        
        int remaining = GetAvailableBullets();
        Debug.Log($"🔫 [BASIC BULLET FIRED] by Player {ownerId}! Bullets remaining in pool: {remaining}");
    }
    
    private int GetAvailableBullets()
    {
        if (BulletPool.Instance != null && ownerId != -1)
        {
            return BulletPool.Instance.GetAvailableBullets(ownerId);
        }
        return 0;
    }
    
    public void OnEquipped(JetsBehaviour jet)
    {
        // Set owner info when equipped
        SetOwnerFromJet(jet);
    }
    
    public void OnUnequipped(JetsBehaviour jet)
    {
        // Basic weapon is never unequipped, no cleanup needed
    }
    
    // Set owner info from JetsBehaviour
    private void SetOwnerFromJet(JetsBehaviour jet)
    {
        if (jet == null) return;
        
        ownerGameObject = jet.gameObject;
        ownerId = jet.playerId;
        ownerName = jet.gameObject.name;
        
        Debug.Log($"🔧 [WEAPON OWNER SET] BasicWeapon owner: Player {ownerId} ({ownerName}) | GameObject: {ownerGameObject.name}");
    }
    
    public int GetCurrentAmmo()
    {
        // Return actual bullets available in pool
        return GetAvailableBullets();
    }
}