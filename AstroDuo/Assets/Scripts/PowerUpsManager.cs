using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class PowerUpsManager : MonoBehaviour
{
    [Header("PowerUp Settings")]
    [SerializeField] private GameObject[] powerUpPrefabs; // Array các prefab power-up
    [SerializeField] private float spawnInterval = 10f; // 10 giây spawn 1 lần
    [SerializeField] private int maxPowerUps = 5; // Số lượng power-up tối đa trên map
    
    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8, -6); // Góc dưới trái
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8, 6);   // Góc trên phải
    
    [Header("Collision Detection")]
    [SerializeField] private LayerMask obstacleLayerMask = -1; // Layer của obstacles
    [SerializeField] private float checkRadius = 0.3f; // Bán kính check collision
    [SerializeField] private int maxSpawnAttempts = 20; // Số lần thử spawn tối đa
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoFindTilemaps = true; // Tự động tìm tilemap
    [SerializeField] private string borderTilemapName = "CannotDestroyBlock"; // Tên GameObject border
    [SerializeField] private string destructibleTilemapName = "CanDestroyBlock"; // Tên GameObject destructible
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showSpawnArea = true; // Hiển thị spawn area trong editor
    
    // Tilemaps
    private Tilemap borderTilemap;
    private Tilemap destructibleTilemap;
    
    // Tracking
    private int currentPowerUpCount = 0;
    private List<GameObject> activePowerUps = new List<GameObject>();
    
    // Available weapon types (excluding Reverse)
    private WeaponType[] availableWeaponTypes = {
        WeaponType.Laser,
        WeaponType.Mine,
        WeaponType.Saber,
        WeaponType.ScatterShot,
        WeaponType.Shield
    };
    
    void Start()
    {
        InitializePowerUpSystem();
    }
    
    void InitializePowerUpSystem()
    {
        // Auto-find tilemaps if enabled
        if (autoFindTilemaps)
        {
            FindTilemaps();
        }
        
        // Load prefabs if not assigned
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            LoadPowerUpPrefabs();
        }
        
        // Start spawning
        StartCoroutine(SpawnPowerUpRoutine());
        
        if (showDebugLogs)
        {
            Debug.Log($"🎁 PowerUpsManager initialized! Available types: {availableWeaponTypes.Length}");
        }
    }
    
    void FindTilemaps()
    {
        // Find border tilemap (CannotDestroyBlock)
        GameObject borderObj = GameObject.FindGameObjectWithTag("CannotDestroyBlock");
        if (borderObj != null)
        {
            borderTilemap = borderObj.GetComponent<Tilemap>();
        }
        
        // Find destructible tilemap (CanDestroyBlock)
        GameObject destructibleObj = GameObject.FindGameObjectWithTag("CanDestroyBlock");
        if (destructibleObj != null)
        {
            destructibleTilemap = destructibleObj.GetComponent<Tilemap>();
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"🗺️ Tilemaps found - Border: {borderTilemap != null}, Destructible: {destructibleTilemap != null}");
        }
    }
    
    void LoadPowerUpPrefabs()
    {
        List<GameObject> loadedPrefabs = new List<GameObject>();
        
        foreach (WeaponType weaponType in availableWeaponTypes)
        {
            string prefabPath = $"Prefabs/{weaponType}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                loadedPrefabs.Add(prefab);
            }
            else if (showDebugLogs)
            {
                Debug.LogWarning($"⚠️ Could not load prefab for {weaponType} at path: {prefabPath}");
            }
        }
        
        powerUpPrefabs = loadedPrefabs.ToArray();
        
        if (showDebugLogs)
        {
            Debug.Log($"🎁 Loaded {powerUpPrefabs.Length} power-up prefabs");
        }
    }
    
    IEnumerator SpawnPowerUpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            if (currentPowerUpCount < maxPowerUps && powerUpPrefabs.Length > 0)
            {
                TrySpawnPowerUp();
            }
        }
    }
    
    void TrySpawnPowerUp()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 randomPosition = GetRandomSpawnPosition();
            
            if (IsValidSpawnPosition(randomPosition))
            {
                SpawnPowerUp(randomPosition);
                return;
            }
        }
        
        if (showDebugLogs)
        {
            Debug.LogWarning($"⚠️ Could not find valid spawn position after {maxSpawnAttempts} attempts");
        }
    }
    
    Vector2 GetRandomSpawnPosition()
    {
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        return new Vector2(x, y);
    }
    
    bool IsValidSpawnPosition(Vector2 position)
    {
        // 1. Check if inside border and not overlapping with border tiles
        if (borderTilemap != null)
        {
            Vector3Int borderCell = borderTilemap.WorldToCell(position);
            if (borderTilemap.HasTile(borderCell))
            {
                return false; // Overlapping with border tile
            }
            
            // Check if inside border area
            if (!IsInsideBorder(position))
            {
                return false; // Outside border area
            }
        }
        
        // 2. Check if not overlapping with destructible blocks
        if (destructibleTilemap != null)
        {
            Vector3Int destructibleCell = destructibleTilemap.WorldToCell(position);
            if (destructibleTilemap.HasTile(destructibleCell))
            {
                return false; // Overlapping with destructible tile
            }
        }
        
        // 3. Check collision with other objects (players, asteroids, other power-ups)
        Collider2D collider = Physics2D.OverlapCircle(position, checkRadius, obstacleLayerMask);
        if (collider != null)
        {
            // Allow spawning if only overlapping with another power-up (will replace)
            PowerUp existingPowerUp = collider.GetComponent<PowerUp>();
            if (existingPowerUp == null)
            {
                return false; // Overlapping with non-power-up object
            }
        }
        
        return true;
    }
    
    bool IsInsideBorder(Vector2 worldPosition)
    {
        if (borderTilemap == null) return true;
        
        Vector3Int cellPos = borderTilemap.WorldToCell(worldPosition);
        
        // Check if position is surrounded by border tiles on all sides
        bool foundLeft = false, foundRight = false, foundUp = false, foundDown = false;
        
        // Check left
        for (int x = cellPos.x; x >= cellPos.x - 15; x--)
        {
            if (borderTilemap.HasTile(new Vector3Int(x, cellPos.y, 0)))
            {
                foundLeft = true;
                break;
            }
        }
        
        // Check right  
        for (int x = cellPos.x; x <= cellPos.x + 15; x++)
        {
            if (borderTilemap.HasTile(new Vector3Int(x, cellPos.y, 0)))
            {
                foundRight = true;
                break;
            }
        }
        
        // Check up
        for (int y = cellPos.y; y <= cellPos.y + 15; y++)
        {
            if (borderTilemap.HasTile(new Vector3Int(cellPos.x, y, 0)))
            {
                foundUp = true;
                break;
            }
        }
        
        // Check down
        for (int y = cellPos.y; y >= cellPos.y - 15; y--)
        {
            if (borderTilemap.HasTile(new Vector3Int(cellPos.x, y, 0)))
            {
                foundDown = true;
                break;
            }
        }
        
        return foundLeft && foundRight && foundUp && foundDown;
    }
    
    void SpawnPowerUp(Vector2 position)
    {
        // Remove existing power-up at this position if any
        Collider2D existingCollider = Physics2D.OverlapCircle(position, checkRadius);
        if (existingCollider != null)
        {
            PowerUp existingPowerUp = existingCollider.GetComponent<PowerUp>();
            if (existingPowerUp != null)
            {
                RemovePowerUp(existingPowerUp.gameObject);
            }
        }
        
        // Choose random power-up type (excluding Reverse)
        WeaponType randomType = availableWeaponTypes[Random.Range(0, availableWeaponTypes.Length)];
        
        // Find matching prefab
        GameObject prefabToSpawn = null;
        foreach (GameObject prefab in powerUpPrefabs)
        {
            PowerUp powerUpComponent = prefab.GetComponent<PowerUp>();
            if (powerUpComponent != null)
            {
                // Check if this prefab matches the selected weapon type
                // We'll need to get the WeaponType from the prefab's PowerUp component
                // Since it's serialized, we'll use the prefab name as a fallback
                if (prefab.name.ToLower().Contains(randomType.ToString().ToLower()))
                {
                    prefabToSpawn = prefab;
                    break;
                }
            }
        }
        
        // Fallback: use random prefab if no match found
        if (prefabToSpawn == null && powerUpPrefabs.Length > 0)
        {
            prefabToSpawn = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        }
        
        if (prefabToSpawn != null)
        {
            GameObject newPowerUp = Instantiate(prefabToSpawn, position, Quaternion.identity);
            activePowerUps.Add(newPowerUp);
            currentPowerUpCount++;
            
            // Set up destruction callback
            PowerUp powerUpComponent = newPowerUp.GetComponent<PowerUp>();
            if (powerUpComponent != null)
            {
                // We'll monitor for destruction in Update instead of using events
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"🎁 Spawned {randomType} power-up at {position}. Total: {currentPowerUpCount}");
            }
        }
    }
    
    void Update()
    {
        // Clean up destroyed power-ups from our tracking list
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            if (activePowerUps[i] == null)
            {
                activePowerUps.RemoveAt(i);
                currentPowerUpCount--;
            }
        }
    }
    
    public void RemovePowerUp(GameObject powerUp)
    {
        if (activePowerUps.Contains(powerUp))
        {
            activePowerUps.Remove(powerUp);
            currentPowerUpCount--;
            Destroy(powerUp);
            
            if (showDebugLogs)
            {
                Debug.Log($"🗑️ Power-up removed. Remaining: {currentPowerUpCount}");
            }
        }
    }
    
    // Public methods for external control
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }
    
    public void SetMaxPowerUps(int max)
    {
        maxPowerUps = max;
    }
    
    public int GetCurrentPowerUpCount()
    {
        return currentPowerUpCount;
    }
    
    public void ForceSpawn()
    {
        if (currentPowerUpCount < maxPowerUps)
        {
            TrySpawnPowerUp();
        }
    }
    
    public void ClearAllPowerUps()
    {
        foreach (GameObject powerUp in activePowerUps)
        {
            if (powerUp != null)
            {
                Destroy(powerUp);
            }
        }
        activePowerUps.Clear();
        currentPowerUpCount = 0;
        
        if (showDebugLogs)
        {
            Debug.Log("🧹 All power-ups cleared!");
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (showSpawnArea)
        {
            // Draw spawn area
            Gizmos.color = Color.green;
            Vector3 center = new Vector3((spawnAreaMin.x + spawnAreaMax.x) / 2, (spawnAreaMin.y + spawnAreaMax.y) / 2, 0);
            Vector3 size = new Vector3(spawnAreaMax.x - spawnAreaMin.x, spawnAreaMax.y - spawnAreaMin.y, 0);
            Gizmos.DrawWireCube(center, size);
            
            // Draw active power-up positions
            Gizmos.color = Color.yellow;
            foreach (GameObject powerUp in activePowerUps)
            {
                if (powerUp != null)
                {
                    Gizmos.DrawWireSphere(powerUp.transform.position, checkRadius);
                }
            }
        }
    }
}
