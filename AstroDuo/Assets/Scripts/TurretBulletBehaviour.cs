using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections;

public class TurretBulletBehaviour : MonoBehaviour
{
    [SerializeField] private float lifetime = 8f; // Shorter lifetime than player bullets
    
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
        Debug.Log($"🎯 Turret Bullet hit: {other.name} | Tag: {other.tag}");
        
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
            Debug.Log("🛡️ Turret Bullet hit indestructible block");
            ReturnToPool();
        }
        else if (other.CompareTag("Jet")) // Handle Jet collision
        {
            HandleJetCollision(other);
        }
        // Keep old Player tag for backward compatibility
        else if (other.CompareTag("Player"))
        {
            Debug.Log("💥 Turret Bullet hit player: " + other.name);
            HandlePlayerHit(other.gameObject);
            ReturnToPool();
        }
        // Handle collision with player bullets (turret bullets destroy player bullets)
        else if (other.CompareTag("Player1Bullet") || other.CompareTag("Player2Bullet"))
        {
            HandlePlayerBulletCollision(other);
        }
    }
    
    // Handle collision with Jet players
    private void HandleJetCollision(Collider2D jetCollider)
    {
        int hitPlayerId = GetPlayerIdFromJet(jetCollider.gameObject);
        
        Debug.Log($"💥 [TURRET HIT] Turret bullet hit Player {hitPlayerId} ({jetCollider.name})");
        
        if (hitPlayerId != -1)
        {
            // NEW: Use turret-specific penalty method
            ApplyTurretPenalty(hitPlayerId);
            
            // NEW: Always end game after turret penalty
            CheckGameEndAfterTurretPenalty();
        }
        else
        {
            Debug.LogWarning($"❓ [UNKNOWN PLAYER] Could not determine player ID for: {jetCollider.name}");
        }
        
        ReturnToPool();
    }
    
    // Handle collision with player bullets
    private void HandlePlayerBulletCollision(Collider2D bulletCollider)
    {
        BasicBulletBehaviour playerBullet = bulletCollider.GetComponent<BasicBulletBehaviour>();
        if (playerBullet != null)
        {
            Debug.Log($"⚡ [TURRET vs PLAYER BULLET] Turret bullet destroyed player bullet");
            
            // Return player bullet to its pool
            playerBullet.ReturnToPool();
        }
        
        // Return this turret bullet to pool
        ReturnToPool();
    }
    
    // Handle legacy player collision
    private void HandlePlayerHit(GameObject player)
    {
        int hitPlayerId = GetPlayerIdFromJet(player);
        
        if (hitPlayerId != -1)
        {
            ApplyTurretPenalty(hitPlayerId);
            CheckGameEndAfterTurretPenalty();
        }
    }
    
    // Get player ID from GameObject
    private int GetPlayerIdFromJet(GameObject jet)
    {
        // First try to get from JetsBehaviour component (more reliable)
        JetsBehaviour jetBehaviour = jet.GetComponent<JetsBehaviour>();
        if (jetBehaviour != null)
        {
            Debug.Log($"🔍 [GET PLAYER ID] From JetsBehaviour: {jet.name} → Player {jetBehaviour.playerId}");
            return jetBehaviour.playerId;
        }
        
        // Fallback to name pattern matching
        string jetName = jet.name.ToLower();
        int playerId = -1;
        
        if (jetName.Contains("player1") || jetName.Contains("p1") || jetName.Contains("jet1"))
        {
            playerId = 1;
        }
        else if (jetName.Contains("player2") || jetName.Contains("p2") || jetName.Contains("jet2"))
        {
            playerId = 2;
        }
        
        Debug.Log($"🔍 [GET PLAYER ID] From name pattern: {jet.name} → Player {playerId}");
        
        if (playerId == -1)
        {
            Debug.LogWarning($"❓ Could not determine player ID for jet: {jetName}");
        }
        
        return playerId;
    }
    
    // Subtract score from specific player
    private void SubtractScoreFromPlayer(int playerId)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is null!");
            return;
        }
        
        if (playerId == 1)
        {
            GameManager.Instance.SubtractPlayer1Score();
        }
        else if (playerId == 2)
        {
            GameManager.Instance.SubtractPlayer2Score();
        }
    }
    
    private bool DestroyTileAtPositionSmart(Tilemap tilemap, Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        
        // Check a 3x3 area around the bullet position
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int checkPos = new Vector3Int(cellPosition.x + x, cellPosition.y + y, 0);
                TileBase tile = tilemap.GetTile(checkPos);
                
                if (tile != null)
                {
                    tilemap.SetTile(checkPos, null);
                    Debug.Log($"✅ SUCCESS: Destroyed tile at {checkPos} (offset: {x},{y})");
                    return true;
                }
            }
        }
        
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
        if (TurretBulletPool.Instance != null)
        {
            TurretBulletPool.Instance.ReturnBullet(gameObject);
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
    
    // Method to reset bullet properties when returned to pool
    public void ResetBullet()
    {
        // Stop any running coroutines
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
        
        Debug.Log("🔄 Turret Bullet reset for pool return");
    }

    // NEW: Apply turret penalty (different from regular SubtractScore)
    private void ApplyTurretPenalty(int playerId)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is null!");
            return;
        }
        
        if (playerId == 1)
        {
            GameManager.Instance.TurretPenaltyPlayer1();
        }
        else if (playerId == 2)
        {
            GameManager.Instance.TurretPenaltyPlayer2();
        }
    }

    // NEW: Check game end after turret penalty
    private void CheckGameEndAfterTurretPenalty()
    {
        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.ShouldEndGameAfterTurretPenalty())
        {
            int player1Score = GameManager.Instance.Player1Score;
            int player2Score = GameManager.Instance.Player2Score;
            
            Debug.Log($"🏁 [TURRET PENALTY] Game ending! P1: {player1Score}, P2: {player2Score}");
            
            // Always go to ScoreScene after turret penalty
            SceneManager.LoadScene(GameConst.SCORE_SCENE);
        }
    }
}