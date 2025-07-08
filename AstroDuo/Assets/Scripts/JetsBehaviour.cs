using System.Collections;
using UnityEngine;

public class JetsBehaviour : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] public float speed = 2f;
    [SerializeField] public float rotationAngle = 20f;
    
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 8f; 
    [SerializeField] private float dashDuration = 0.2f; 
    [SerializeField] private float doubleTapTimeThreshold = 0.3f;
    [SerializeField] private float dashArcAngle = 45f; 
    [SerializeField] private float dashArcIntensity = 1f; 
    [SerializeField] private float dashCooldown = 0.8f; // ← NEW: Dash cooldown
    [SerializeField] private KeyCode moveKey = KeyCode.LeftArrow; 
    [SerializeField] private KeyCode fireKey = KeyCode.UpArrow; 

    [Header("Weapon System")]
    [SerializeField] private Transform firePoint;

    private float lastLeftArrowPressTime;
    private bool isDashing = false;
    private Coroutine currentDashCoroutine; // ← NEW: Track current dash
    private float lastDashTime = -999f; // ← NEW: Track last dash time
    
    // Weapon system
    private IWeapon currentWeapon;
    private BasicWeapon basicWeapon;
    
    void Start()
    {
        // fixed framerate at 60fps
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; 
        
        // Initialize weapon system
        basicWeapon = new BasicWeapon();
        currentWeapon = basicWeapon;
        
        // Find FirePoint if not assigned
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
            if (firePoint == null)
            {
                Debug.LogError("FirePoint not found! Please create a child object named 'FirePoint'");
            }
        }
    }

    void Update()
    {
        if (isDashing)
        {
            return; // Cannot change direction while dashing
        }

        AutoMoveForward();

        // Movement input
        if (Input.GetKeyDown(moveKey))
        {
            // click 2 times - Dash
            if (Time.time - lastLeftArrowPressTime < doubleTapTimeThreshold)
            {
                TryStartDash(); // ← NEW: Use safe dash method
            }
            else
            {
                // If only click 1 time, rotate
                ChangeDirection();
            }

            lastLeftArrowPressTime = Time.time;
        }
        
        // Fire input
        if (Input.GetKeyDown(fireKey))
        {
            FireCurrentWeapon();
        }
    }
    
    private void FireCurrentWeapon()
    {
        if (currentWeapon == null || firePoint == null) 
        {
            Debug.Log("❌ Cannot fire: No weapon or firepoint!");
            return;
        }
        
        // LOG: Show current weapon status BEFORE firing
        Debug.Log($"🎯 Attempting to fire {currentWeapon.WeaponType} | Can fire: {currentWeapon.CanFire()}");
        
        if (currentWeapon.CanFire())
        {
            currentWeapon.Fire(firePoint, transform.up);
            
            // Check if single-use weapon was used up
            if (currentWeapon != basicWeapon && !currentWeapon.CanFire())
            {
                Debug.Log($"🔄 {currentWeapon.WeaponType} used up! Switching back to BasicWeapon");
                SwitchToBasicWeapon();
            }
        }
        else
        {
            Debug.Log($"⚠️ {currentWeapon.WeaponType} cannot fire! (Already used or on cooldown)");
        }
    }
    
    public void EquipWeapon(IWeapon newWeapon)
    {
        if (newWeapon == null) 
        {
            Debug.LogError("❌ Trying to equip null weapon!");
            return;
        }
        
        WeaponType oldWeaponType = currentWeapon?.WeaponType ?? WeaponType.BasicBullet;
        
        // Unequip current weapon (if it's not basic weapon)
        if (currentWeapon != null && currentWeapon != basicWeapon)
        {
            currentWeapon.OnUnequipped(this);
            Debug.Log($"📤 Old weapon {oldWeaponType} unequipped");
        }
        
        // Equip new weapon
        currentWeapon = newWeapon;
        currentWeapon.OnEquipped(this);
        
        Debug.Log($"🔧 WEAPON EQUIPPED: {oldWeaponType} → {currentWeapon.WeaponType}");
    }
    
    private void SwitchToBasicWeapon()
    {
        if (currentWeapon != basicWeapon)
        {
            WeaponType oldWeapon = currentWeapon.WeaponType;
            currentWeapon.OnUnequipped(this);
            currentWeapon = basicWeapon;
            Debug.Log($"🔙 REVERTED TO BASIC: {oldWeapon} → BasicBullet");
        }
    }
    
    public int GetCurrentAmmo()
    {
        if (currentWeapon == basicWeapon)
        {
            return basicWeapon.GetCurrentAmmo();
        }
        return currentWeapon.CanFire() ? 1 : 0; // Single use weapons
    }
    
    public WeaponType GetCurrentWeaponType()
    {
        return currentWeapon?.WeaponType ?? WeaponType.BasicBullet;
    }

    // Detect collision với blocks
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("CanDestroyBlock"))
        {
            Debug.Log("Player va chạm với CanDestroyBlock: " + collision.gameObject.name);
        }
        else if (collision.gameObject.CompareTag("CannotDestroyBlock"))
        {
            Debug.Log("Player va chạm với CannotDestroyBlock: " + collision.gameObject.name);
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        float startTime = Time.time;
        
        int rotationSteps = 2;
        bool[] rotationCompleted = new bool[rotationSteps];

        try // ← NEW: Safety wrapper
        {
            while (Time.time < startTime + dashDuration)
            {
                float progress = (Time.time - startTime) / dashDuration;
                
                Vector3 forwardMovement = transform.up * dashSpeed * Time.deltaTime;
                float currentAngle = Mathf.Lerp(0, dashArcAngle * Mathf.Deg2Rad, progress);
                float radius = dashSpeed * dashDuration / dashArcIntensity;
                
                Vector3 arcOffset = new Vector3(
                    -Mathf.Sin(currentAngle) * radius * Time.deltaTime,
                    0,
                    0
                );
                
                Vector3 rotatedOffset = Quaternion.Euler(0, 0, transform.eulerAngles.z) * arcOffset;
                transform.position += forwardMovement + rotatedOffset;
                
                for (int i = 0; i < rotationSteps; i++)
                {
                    float stepThreshold = (float)(i + 1) / rotationSteps;
                    if (progress >= stepThreshold && !rotationCompleted[i])
                    {
                        float stepRotation = dashArcAngle / rotationSteps;
                        transform.Rotate(0, 0, stepRotation);
                        rotationCompleted[i] = true;
                    }
                }
                
                yield return null;
            }
        }
        finally // ← NEW: Ensure cleanup
        {
            isDashing = false;
            currentDashCoroutine = null;
            Debug.Log("✅ Dash completed!");
        }
    }

    private void AutoMoveForward()
    {   
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void ChangeDirection()
    {   
        transform.Rotate(0, 0, rotationAngle);
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Draw bullet position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            
            // Draw search radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }

    // NEW: Public method to check dash availability
    public bool CanDash()
    {
        return !isDashing && (Time.time - lastDashTime >= dashCooldown);
    }
    
    // NEW: Get dash cooldown remaining
    public float GetDashCooldownRemaining()
    {
        if (CanDash()) return 0f;
        return dashCooldown - (Time.time - lastDashTime);
    }

    private void TryStartDash() // ← NEW: Safe dash starter
    {
        // Check if dash is on cooldown
        if (Time.time - lastDashTime < dashCooldown)
        {
            Debug.Log($"⏱️ Dash on cooldown! {dashCooldown - (Time.time - lastDashTime):F1}s remaining");
            return;
        }
        
        // Check if already dashing
        if (isDashing)
        {
            Debug.Log("⚠️ Already dashing! Cannot start new dash.");
            return;
        }
        
        // Stop any existing dash coroutine (safety)
        if (currentDashCoroutine != null)
        {
            StopCoroutine(currentDashCoroutine);
            currentDashCoroutine = null;
        }
        
        // Start new dash
        currentDashCoroutine = StartCoroutine(Dash());
        lastDashTime = Time.time;
        Debug.Log("🚀 Dash started!");
    }
}
