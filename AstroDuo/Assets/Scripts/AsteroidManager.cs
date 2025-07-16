using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class AsteroidManager : MonoBehaviour
{
    [Header("Asteroid Settings")]
    [SerializeField] private GameObject[] asteroidPrefabs; // Array các prefab asteroid
    [SerializeField] private float spawnInterval = 5f; // Thời gian spawn (giây)
    [SerializeField] private int maxAsteroids = 10; // Số lượng asteroid tối đa
    
    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-10, -5); // Góc dưới trái
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(10, 5);   // Góc trên phải
    
    [Header("Collision Detection")]
    [SerializeField] private Tilemap borderTilemap; // Tilemap hồng (viền)
    [SerializeField] private Tilemap destructibleTilemap; // Tilemap vàng (có thể phá hủy)
    [SerializeField] private LayerMask obstacleLayerMask = -1; // Layer của obstacles
    [SerializeField] private float checkRadius = 0.5f; // Bán kính check collision
    
    [Header("Debug")]
    [SerializeField] private bool showSpawnArea = true; // Hiển thị spawn area trong editor
    
    private int currentAsteroidCount = 0;
    
    void Start()
    {
        // Bắt đầu spawn asteroid
        StartCoroutine(SpawnAsteroidRoutine());
    }
    
    IEnumerator SpawnAsteroidRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            if (currentAsteroidCount < maxAsteroids && asteroidPrefabs.Length > 0)
            {
                TrySpawnAsteroid();
            }
        }
    }
    
    void TrySpawnAsteroid()
    {
        int attempts = 0;
        int maxAttempts = 20; // Giới hạn số lần thử spawn
        
        while (attempts < maxAttempts)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();
            
            if (IsValidSpawnPosition(spawnPosition))
            {
                SpawnAsteroid(spawnPosition);
                break;
            }
            
            attempts++;
        }
        
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("⚠️ Không thể tìm được vị trí spawn hợp lệ cho asteroid!");
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
        Vector3Int cellPosition = Vector3Int.FloorToInt(position);
        
        // 1. PHẢI nằm TRONG viền hồng
        if (borderTilemap != null)
        {
            Vector3Int borderCell = borderTilemap.WorldToCell(position);
            if (borderTilemap.HasTile(borderCell))
            {
                return false; // Trùng với block viền hồng
            }
            
            // Check xem có nằm trong khu vực được bao quanh bởi viền không
            if (!IsInsideBorder(position))
            {
                return false; // Nằm ngoài viền
            }
        }
        
        // 2. KHÔNG được trùng với block vàng
        if (destructibleTilemap != null)
        {
            Vector3Int destructibleCell = destructibleTilemap.WorldToCell(position);
            if (destructibleTilemap.HasTile(destructibleCell))
            {
                return false; // Trùng với block vàng
            }
        }
        
        // 3. Check collision với các object khác
        Collider2D collider = Physics2D.OverlapCircle(position, checkRadius, obstacleLayerMask);
        return collider == null;
    }
    
    bool IsInsideBorder(Vector2 worldPosition)
    {
        // Convert to tilemap coordinates
        Vector3Int cellPos = borderTilemap.WorldToCell(worldPosition); 
        
        // Check 4 hướng từ vị trí này ra ngoài
        // Nếu tất cả hướng đều gặp border tile thì position này nằm trong viền
        
        bool foundLeft = false, foundRight = false, foundUp = false, foundDown = false;
        
        // Check left
        for (int x = cellPos.x; x >= cellPos.x - 20; x--)
        {
            if (borderTilemap.HasTile(new Vector3Int(x, cellPos.y, 0)))
            {
                foundLeft = true;
                break;
            }
        }
        
        // Check right  
        for (int x = cellPos.x; x <= cellPos.x + 20; x++)
        {
            if (borderTilemap.HasTile(new Vector3Int(x, cellPos.y, 0)))
            {
                foundRight = true;
                break;
            }
        }
        
        // Check up
        for (int y = cellPos.y; y <= cellPos.y + 20; y++)
        {
            if (borderTilemap.HasTile(new Vector3Int(cellPos.x, y, 0)))
            {
                foundUp = true;
                break;
            }
        }
        
        // Check down
        for (int y = cellPos.y; y >= cellPos.y - 20; y--)
        {
            if (borderTilemap.HasTile(new Vector3Int(cellPos.x, y, 0)))
            {
                foundDown = true;
                break;
            }
        }
        
        return foundLeft && foundRight && foundUp && foundDown;
    }
    
    void SpawnAsteroid(Vector2 position)
    {
        // Random chọn 1 prefab từ array
        int randomIndex = Random.Range(0, asteroidPrefabs.Length);
        GameObject asteroidPrefab = asteroidPrefabs[randomIndex];
        
        // Spawn asteroid
        GameObject asteroid = Instantiate(asteroidPrefab, position, Quaternion.identity);
        
        // Random rotation ban đầu
        float randomRotation = Random.Range(0f, 360f);
        asteroid.transform.rotation = Quaternion.Euler(0, 0, randomRotation);
        
        // Tăng counter
        currentAsteroidCount++;
        
        // Đăng ký event khi asteroid bị destroy
        AsteroidBehaviour asteroidBehaviour = asteroid.GetComponent<AsteroidBehaviour>();
        if (asteroidBehaviour != null)
        {
            asteroidBehaviour.OnAsteroidDestroyed += OnAsteroidDestroyed;
        }
        
        Debug.Log($"🌑 Asteroid spawned at {position} - Total: {currentAsteroidCount}");
    }
    
    void OnAsteroidDestroyed()
    {
        currentAsteroidCount--;
        Debug.Log($"💥 Asteroid destroyed - Remaining: {currentAsteroidCount}");
    }
    
    // Spawn asteroid manually (có thể gọi từ script khác)
    public void SpawnAsteroidManually()
    {
        if (currentAsteroidCount < maxAsteroids)
        {
            TrySpawnAsteroid();
        }
    }
    
    // Clear tất cả asteroid
    public void ClearAllAsteroids()
    {
        AsteroidBehaviour[] asteroids = FindObjectsOfType<AsteroidBehaviour>();
        // AsteroidBehaviour[] asteroids = FindObjectsByType<AsteroidBehaviour>();
        foreach (var asteroid in asteroids)
        {
            Destroy(asteroid.gameObject);
        }
        currentAsteroidCount = 0;
    }
    
    void OnDrawGizmosSelected()
    {
        if (showSpawnArea)
        {
            // Vẽ spawn area trong Scene view
            Gizmos.color = Color.yellow;
            Vector2 center = (spawnAreaMin + spawnAreaMax) / 2;
            Vector2 size = spawnAreaMax - spawnAreaMin;
            Gizmos.DrawWireCube(center, size);
            
            // Vẽ check radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, checkRadius);
        }
    }
} 