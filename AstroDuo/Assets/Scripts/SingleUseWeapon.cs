using UnityEngine;

public abstract class SingleUseWeapon : IWeapon
{
    public abstract WeaponType WeaponType { get; }
    protected bool hasBeenUsed = false;
    
    public bool CanFire()
    {
        return !hasBeenUsed;
    }
    
    public void Fire(Transform firePoint, Vector3 direction)
    {
        if (!CanFire()) 
        {
            Debug.Log($"❌ {WeaponType} already used!");
            return;
        }
        
        Debug.Log($"🔥 FIRING {WeaponType}! (Single use weapon)");
        ExecuteFire(firePoint, direction);
        hasBeenUsed = true;
        Debug.Log($"✅ {WeaponType} used successfully! Will switch to basic weapon.");
    }
    
    protected abstract void ExecuteFire(Transform firePoint, Vector3 direction);
    
    public virtual void OnEquipped(JetsBehaviour jet)
    {
        Debug.Log($"⚡ {WeaponType} equipped! Single use available.");
    }
    
    public virtual void OnUnequipped(JetsBehaviour jet)
    {
        Debug.Log($"📤 {WeaponType} unequipped!");
    }
}
