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
        
        Debug.Log($"Basic bullet fired! Ammo remaining: {currentAmmo}");
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
        // Basic weapon is always equipped, no special setup needed
    }
    
    public void OnUnequipped(JetsBehaviour jet)
    {
        // Basic weapon is never unequipped, no cleanup needed
    }
    
    public int GetCurrentAmmo()
    {
        UpdateAmmo();
        return currentAmmo;
    }
}