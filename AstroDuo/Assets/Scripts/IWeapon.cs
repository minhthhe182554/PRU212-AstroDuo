using UnityEngine;

public interface IWeapon
{
    WeaponType WeaponType { get; }
    bool CanFire();
    void Fire(Transform firePoint, Vector3 direction);
    void OnEquipped(JetsBehaviour jet);
    void OnUnequipped(JetsBehaviour jet);
}

public enum WeaponType
{
    BasicBullet,    
    Laser,          
    Mine,           
    Saber,          
    ScatterShot,      
    Reverse,      
    Shield      
}