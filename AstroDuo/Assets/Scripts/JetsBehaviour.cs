using System.Collections;
using UnityEngine;

public class JetsBehaviour : MonoBehaviour
{
    [Header("Player Info")] // NEW: Player identification
    [SerializeField] public int playerId = 1; // 1 for Player1, 2 for Player2
    
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

    [Header("Auto Reverse System")]
    [SerializeField] private float reverseInterval = 20f; // 20 seconds

    [Header("Weapon System")]
    [SerializeField] private Transform firePoint;

    private float lastLeftArrowPressTime;
    private bool isDashing = false;
    private Coroutine currentDashCoroutine; // ← NEW: Track current dash
    private float lastDashTime = -999f; // ← NEW: Track last dash time
    
    // Auto-reverse system - PER MAP (not global)
    private static float mapStartTime = 0f;
    private static bool isReversed = false;
    private static bool mapStartTimeSet = false;
    private static string currentMapName = "";
    
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
        
        // NEW: Set weapon owner immediately
        basicWeapon.OnEquipped(this);
        
        // Find FirePoint if not assigned
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
            if (firePoint == null)
            {
                Debug.LogError("FirePoint not found! Please create a child object named 'FirePoint'");
            }
        }
        
        // Initialize map timer (resets for each new map)
        InitializeMapTimer();
        
        Debug.Log($"🚀 Player {playerId} ({gameObject.name}) initialized!");
    }
    
    private void InitializeMapTimer()
    {
        string newMapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Check if this is a new map or first initialization
        if (!mapStartTimeSet || currentMapName != newMapName)
        {
            mapStartTime = Time.time;
            mapStartTimeSet = true;
            currentMapName = newMapName;
            isReversed = false; // Always start with normal direction on new map
            
            Debug.Log($"🗺️ Map timer initialized for '{currentMapName}' at {mapStartTime}");
        }
    }

    void Update()
    {
        // Check for automatic reverse every frame
        UpdateAutoReverse();
        
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
    
    private void UpdateAutoReverse()
    {
        // Calculate how many reverse intervals have passed since this map started
        float timeSinceMapStart = Time.time - mapStartTime;
        int reverseCount = Mathf.FloorToInt(timeSinceMapStart / reverseInterval);
        
        // Check if we should be reversed (odd number of intervals)
        bool shouldBeReversed = (reverseCount % 2 == 1);
        
        // If reverse state changed, update it
        if (shouldBeReversed != isReversed)
        {
            isReversed = shouldBeReversed;
            
            // Only show UI and log when reverse is activated (not when deactivated)
            if (isReversed)
            {
                float timeInMap = Time.time - mapStartTime;
                Debug.Log($"🔄 AUTO-REVERSE ACTIVATED in {currentMapName} after {timeInMap:F1}s!");
                
                // Show UI for reverse activation
                if (ReverseUIManager.Instance != null)
                {
                    ReverseUIManager.Instance.ShowReverse();
                }
            }
            else
            {
                float timeInMap = Time.time - mapStartTime;
                Debug.Log($"🔄 AUTO-REVERSE DEACTIVATED in {currentMapName} after {timeInMap:F1}s!");
                // No UI shown for deactivation
            }
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

        // Apply reverse multiplier if needed
        float dashAngleMultiplier = isReversed ? -1f : 1f;

        try // ← NEW: Safety wrapper
        {
            while (Time.time < startTime + dashDuration)
            {
                float progress = (Time.time - startTime) / dashDuration;
                
                Vector3 forwardMovement = transform.up * dashSpeed * Time.deltaTime;
                float currentAngle = Mathf.Lerp(0, dashArcAngle * dashAngleMultiplier * Mathf.Deg2Rad, progress);
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
                        float stepRotation = (dashArcAngle * dashAngleMultiplier) / rotationSteps;
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
        // Apply reverse multiplier if needed
        float rotationMultiplier = isReversed ? -1f : 1f;
        transform.Rotate(0, 0, rotationAngle * rotationMultiplier);
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
            
            // Draw reverse indicator
            if (isReversed)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            }
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
    
    // NEW: Get reverse status
    public bool IsReversed()
    {
        return isReversed;
    }
    
    // NEW: Get time until next reverse in current map
    public float GetTimeUntilNextReverse()
    {
        float timeSinceMapStart = Time.time - mapStartTime;
        float timeInCurrentInterval = timeSinceMapStart % reverseInterval;
        return reverseInterval - timeInCurrentInterval;
    }
    
    // NEW: Get current map time
    public float GetCurrentMapTime()
    {
        return Time.time - mapStartTime;
    }
    
    // NEW: Get current map name
    public string GetCurrentMapName()
    {
        return currentMapName;
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
        Debug.Log($"🚀 Dash started! {(isReversed ? "(REVERSED)" : "(NORMAL)")}");
    }
}
