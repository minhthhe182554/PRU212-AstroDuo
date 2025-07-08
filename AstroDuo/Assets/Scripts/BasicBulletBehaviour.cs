using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class BasicBulletBehaviour : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    private Coroutine lifetimeCoroutine;
    
    void OnEnable()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }
        lifetimeCoroutine = StartCoroutine(LifetimeCountdown());
    }
    
    void OnDisable()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
    }
    
    private IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);
        ReturnToPool();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🎯 Bullet hit: {other.name} | Tag: {other.tag}");
        
        if (other.CompareTag("CanDestroyBlock"))
        {
            Tilemap tilemap = other.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                Debug.Log($"📍 Tilemap found: {tilemap.name}");
                
                // Method 1: Use bullet position with smart search
                bool destroyed = DestroyTileAtPositionSmart(tilemap, transform.position);
                
                if (!destroyed)
                {
                    // Method 2: Use collider bounds overlap
                    destroyed = DestroyTileUsingBounds(tilemap, other);
                }
                
                if (!destroyed)
                {
                    Debug.LogWarning("❌ Could not destroy any tile!");
                }
            }
            else
            {
                Debug.Log("❌ No Tilemap component found!");
                Destroy(other.gameObject);
            }
            ReturnToPool();
        }
        else if (other.CompareTag("CannotDestroyBlock"))
        {
            Debug.Log("🛡️ Bullet hit indestructible block");
            ReturnToPool();
        }
        else if (other.CompareTag("Player"))
        {
            Debug.Log("💀 Bullet hit enemy jet: " + other.name);
            Destroy(other.gameObject);
            ReturnToPool();
        }
    }
    
    private bool DestroyTileAtPositionSmart(Tilemap tilemap, Vector3 worldPosition)
    {
        // Try multiple positions around bullet with larger search radius
        Vector3[] testPositions = {
            worldPosition,                                    // Center
            worldPosition + Vector3.up * 0.2f,              // Up
            worldPosition + Vector3.down * 0.2f,            // Down  
            worldPosition + Vector3.left * 0.2f,            // Left
            worldPosition + Vector3.right * 0.2f,           // Right
            worldPosition + new Vector3(0.2f, 0.2f, 0),     // Top-right
            worldPosition + new Vector3(-0.2f, 0.2f, 0),    // Top-left
            worldPosition + new Vector3(0.2f, -0.2f, 0),    // Bottom-right
            worldPosition + new Vector3(-0.2f, -0.2f, 0),   // Bottom-left
        };
        
        foreach (Vector3 testPos in testPositions)
        {
            Vector3Int tilePosition = tilemap.WorldToCell(testPos);
            TileBase tileAtPosition = tilemap.GetTile(tilePosition);
            
            if (tileAtPosition != null)
            {
                tilemap.SetTile(tilePosition, null);
                Debug.Log($"✅ SUCCESS: Destroyed tile at {tilePosition} using position {testPos}");
                return true;
            }
        }
        
        Debug.Log($"❌ No tile found around {worldPosition}");
        return false;
    }
    
    private bool DestroyTileUsingBounds(Tilemap tilemap, Collider2D tilemapCollider)
    {
        // Get bullet's collider
        Collider2D bulletCollider = GetComponent<Collider2D>();
        if (bulletCollider == null) return false;
        
        // Find overlap area
        Bounds bulletBounds = bulletCollider.bounds;
        Bounds tilemapBounds = tilemapCollider.bounds;
        
        // Check tiles in overlap area
        Vector3 minWorld = new Vector3(
            Mathf.Max(bulletBounds.min.x, tilemapBounds.min.x),
            Mathf.Max(bulletBounds.min.y, tilemapBounds.min.y),
            0
        );
        
        Vector3 maxWorld = new Vector3(
            Mathf.Min(bulletBounds.max.x, tilemapBounds.max.x),
            Mathf.Min(bulletBounds.max.y, tilemapBounds.max.y),
            0
        );
        
        Vector3Int minTile = tilemap.WorldToCell(minWorld);
        Vector3Int maxTile = tilemap.WorldToCell(maxWorld);
        
        // Search in bounds area
        for (int x = minTile.x; x <= maxTile.x; x++)
        {
            for (int y = minTile.y; y <= maxTile.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(tilePos);
                
                if (tile != null)
                {
                    tilemap.SetTile(tilePos, null);
                    Debug.Log($"✅ SUCCESS: Destroyed tile at {tilePos} using bounds method");
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private void ReturnToPool()
    {
        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnBecameInvisible()
    {
        ReturnToPool();
    }
}
