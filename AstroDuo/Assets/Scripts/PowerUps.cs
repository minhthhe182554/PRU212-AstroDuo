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
            case WeaponType.Reverse:
                return new ReverseWeapon();
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

public class ReverseWeapon : SingleUseWeapon
{
    public override WeaponType WeaponType => WeaponType.Reverse;
    
    protected override void ExecuteFire(Transform firePoint, Vector3 direction)
    {
        Debug.Log("🔄 REVERSE SHOT! *backwards projectile*");
        // TODO: Implement reverse shot logic later
    }
}

public class SaberWeapon : SingleUseWeapon
{
    public override WeaponType WeaponType => WeaponType.Saber;
    
    protected override void ExecuteFire(Transform firePoint, Vector3 direction)
    {
        Debug.Log("⚔️ SABER STRIKE! *melee attack*");
        // TODO: Implement saber melee logic later
    }
}

public class ScatterWeapon : SingleUseWeapon
{
    public override WeaponType WeaponType => WeaponType.ScatterShot;
    
    protected override void ExecuteFire(Transform firePoint, Vector3 direction)
    {
        Debug.Log("💥 SCATTER SHOT! *multiple projectiles*");
        // TODO: Implement scatter shot logic later
    }
}

public class ShieldWeapon : SingleUseWeapon
{
    public override WeaponType WeaponType => WeaponType.Shield;
    
    protected override void ExecuteFire(Transform firePoint, Vector3 direction)
    {
        Debug.Log("🛡️ SHIELD ACTIVATED! *protective barrier*");
        // TODO: Implement shield logic later
    }
}