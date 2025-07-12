using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWeapon : IWeapon
{
    public WeaponType WeaponType => WeaponType.BasicBullet;
    
    private int maxAmmo = 3;
    private float bulletCooldown = 1f;
    private float bulletSpeed = 10f;
    private float fireDelay = 0.05f;
    
    private int currentAmmo;
    private Queue<float> bulletCooldownQueue;
    private float lastFireTime;
    
    // Track owner info
    private int ownerId = -1;
    private string ownerName = "";
    private GameObject ownerGameObject; // NEW: Owner GameObject reference
    
    public BasicWeapon()
    {
        currentAmmo = maxAmmo;
        bulletCooldownQueue = new Queue<float>();
        lastFireTime = 0f;
    }
    
    public bool CanFire()
    {
        UpdateAmmo();
        return currentAmmo > 0 && Time.time - lastFireTime >= fireDelay;
    }
    
    public void Fire(Transform firePoint, Vector3 direction)
    {
        if (!CanFire()) return;
        
        // Get bullet from pool
        if (BulletPool.Instance == null) return;
        
        GameObject bullet = BulletPool.Instance.GetBasicBullet();
        if (bullet == null) return;
        
        // Setup bullet
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        
        // NEW: Set bullet owner with GameObject reference
        BasicBulletBehaviour bulletBehaviour = bullet.GetComponent<BasicBulletBehaviour>();
        if (bulletBehaviour != null)
        {
            bulletBehaviour.SetOwner(ownerId, ownerName, ownerGameObject);
        }
        
        // Fire bullet
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
        
        // Update ammo
        currentAmmo--;
        bulletCooldownQueue.Enqueue(Time.time + bulletCooldown);
        lastFireTime = Time.time;
        
        Debug.Log($"Basic bullet fired by Player {ownerId}! Ammo remaining: {currentAmmo}");
    }
    
    private void UpdateAmmo()
    {
        while (bulletCooldownQueue.Count > 0 && Time.time >= bulletCooldownQueue.Peek())
        {
            bulletCooldownQueue.Dequeue();
            currentAmmo++;
        }
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
        
        ownerGameObject = jet.gameObject; // NEW: Store GameObject reference
        ownerId = jet.playerId; // NEW: Use playerId from JetsBehaviour
        ownerName = jet.gameObject.name;
        
        Debug.Log($"🔧 BasicWeapon owner set: Player {ownerId} ({ownerName})");
    }
    
    public int GetCurrentAmmo()
    {
        UpdateAmmo();
        return currentAmmo;
    }
}