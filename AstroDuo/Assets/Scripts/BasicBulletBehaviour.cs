using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections;

public class BasicBulletBehaviour : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float ignoreOwnerDuration = 0.15f; // NEW: Time to ignore owner collision
    
    private Coroutine lifetimeCoroutine;
    
    // NEW: Track bullet owner
    public int ownerId = -1; // -1 = no owner, 1 = Player1, 2 = Player2
    public string ownerName = ""; // For debugging
    public GameObject ownerGameObject; // NEW: Reference to owner GameObject
    private float spawnTime; // NEW: Track when bullet was spawned
    
    void OnEnable()
    {
        // DON'T set spawnTime here - wait until SetOwner is called
        
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
    
    // NEW: Method to set bullet owner with GameObject reference
    public void SetOwner(int playerId, string playerName, GameObject ownerGO)
    {
        ownerId = playerId;
        ownerName = playerName;
        ownerGameObject = ownerGO;
        spawnTime = Time.time; // MOVE spawnTime setting HERE to ensure proper timing
        Debug.Log($"🎯 [BULLET OWNER SET] Player {playerId} ({playerName}) | SpawnTime: {spawnTime}");
    }
    
    // NEW: Check if should ignore collision with specific object
    private bool ShouldIgnoreCollision(GameObject hitObject)
    {
        // SAFETY CHECK: Make sure owner has been set
        if (ownerId == -1 || ownerGameObject == null)
        {
            Debug.LogWarning($"⚠️ [COLLISION WARNING] Bullet owner not set yet! Ignoring collision with {hitObject.name}");
            return true; // Ignore collision if owner not set yet
        }
        
        // Ignore collision with owner for a short time after spawn
        if (ownerGameObject != null && hitObject == ownerGameObject)
        {
            float timeSinceSpawn = Time.time - spawnTime;
            if (timeSinceSpawn < ignoreOwnerDuration)
            {
                Debug.Log($"🚫 [COLLISION IGNORED] Owner collision ignored (time: {timeSinceSpawn:F2}s)");
                return true;
            }
        }
        
        // NEW: Ignore collision with bullets from the same player
        string currentPlayerBulletTag = $"Player{ownerId}Bullet";
        if (hitObject.CompareTag(currentPlayerBulletTag))
        {
            Debug.Log($"🚫 [BULLET COLLISION IGNORED] Same player bullet collision: {hitObject.name}");
            return true;
        }
        
        return false;
    }
    
    private IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);
        ReturnToPool();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // NEW: Safety check - if owner not set yet, ignore all collisions
        if (ownerId == -1)
        {
            Debug.LogWarning($"⚠️ [EARLY COLLISION] Bullet hit {other.name} before owner was set! Ignoring.");
            return;
        }
        
        // NEW: Check if should ignore this collision
        if (ShouldIgnoreCollision(other.gameObject))
        {
            return; // Skip this collision
        }
        
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
        else if (other.CompareTag("Jet")) // Handle Jet collision
        {
            HandleJetCollision(other);
        }
        // Keep old Player tag for backward compatibility
        else if (other.CompareTag("Player"))
        {
            Debug.Log("💀 Bullet hit enemy jet: " + other.name);
            Destroy(other.gameObject);
            ReturnToPool();
        }
        // NEW: Handle collision with enemy bullets
        else if (other.CompareTag("Player1Bullet") || other.CompareTag("Player2Bullet"))
        {
            HandleBulletCollision(other);
        }
    }
    
    // Handle collision with Jet players
    private void HandleJetCollision(Collider2D jetCollider)
    {
        // Get hit player's ID from their name or component
        int hitPlayerId = GetPlayerIdFromJet(jetCollider.gameObject);
        
        Debug.Log($"💥 [JET COLLISION] Bullet Owner: Player {ownerId} ({ownerName}) | Hit Player: {hitPlayerId} ({jetCollider.name})");
        
        // Check if bullet hit a different player (not self)
        if (ownerId != -1 && hitPlayerId != -1 && ownerId != hitPlayerId)
        {
            // Bullet owner gets a point
            AddScoreToPlayer(ownerId);
            Debug.Log($"⭐ [VALID HIT] Player {ownerId} scored! Hit Player {hitPlayerId}");
            
            // Check if game should end
            CheckGameEnd();
        }
        else if (ownerId == hitPlayerId)
        {
            // NEW: Self-shot - subtract score if player has points
            Debug.Log($"🤡 [SELF HIT] Player {ownerId} shot themselves! Owner={ownerId}, Hit={hitPlayerId}");
            SubtractScoreFromPlayer(ownerId);
            
            // Still check if game should end (in case we want to handle end conditions)
            CheckGameEnd();
        }
        else
        {
            Debug.LogWarning($"❓ [UNKNOWN COLLISION] Owner={ownerId}, Hit={hitPlayerId} | Names: {ownerName} → {jetCollider.name}");
        }
        
        ReturnToPool();
    }
    
    // NEW: Handle collision with enemy bullets
    private void HandleBulletCollision(Collider2D bulletCollider)
    {
        BasicBulletBehaviour otherBullet = bulletCollider.GetComponent<BasicBulletBehaviour>();
        if (otherBullet == null) return;
        
        int otherBulletOwnerId = otherBullet.ownerId;
        
        Debug.Log($"💥 [BULLET COLLISION] Player {ownerId} bullet hit Player {otherBulletOwnerId} bullet");
        
        // Both bullets are destroyed when they collide (if from different players)
        if (ownerId != otherBulletOwnerId && ownerId != -1 && otherBulletOwnerId != -1)
        {
            Debug.Log($"⚡ [BULLET CLASH] Both bullets destroyed! Player {ownerId} vs Player {otherBulletOwnerId}");
            
            // Return other bullet to its pool
            otherBullet.ReturnToPool();
            
            // Return this bullet to pool
            ReturnToPool();
        }
        else
        {
            Debug.Log($"🚫 [BULLET IGNORED] Same player bullet or invalid owner IDs");
        }
    }
    
    // Get player ID from GameObject name or component
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
    
    // Add score to specific player
    private void AddScoreToPlayer(int playerId)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is null!");
            return;
        }
        
        if (playerId == 1)
        {
            GameManager.Instance.AddPlayer1Score();
        }
        else if (playerId == 2)
        {
            GameManager.Instance.AddPlayer2Score();
        }
    }

    // NEW: Subtract score from specific player (only if they have points)
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
    
    // Check if game should end and switch to ScoreScene
    private void CheckGameEnd()
    {
        if (GameManager.Instance == null) return;
        
        int player1Score = GameManager.Instance.Player1Score;
        int player2Score = GameManager.Instance.Player2Score;
        
        // NEW: Only switch to ScoreScene when someone scores, not every hit
        // Check if anyone reached max score OR we want to show score after each point
        Debug.Log($"🏁 Point scored! P1: {player1Score}, P2: {player2Score}");
        
        // Always go to ScoreScene after each point to show progress
        SceneManager.LoadScene(GameConst.SCORE_SCENE);
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
    
    public void ReturnToPool()
    {
        // Store playerId before resetting for pool return
        int playerIdForReturn = ownerId;
        
        if (BulletPool.Instance != null && playerIdForReturn != -1)
        {
            BulletPool.Instance.ReturnBullet(gameObject, playerIdForReturn);
        }
        else if (BulletPool.Instance != null)
        {
            // Fallback to legacy method if playerId not set
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
    
    // NEW: Method to reset bullet properties when returned to pool
    public void ResetBullet()
    {
        // Reset owner info
        ownerId = -1;
        ownerName = "";
        ownerGameObject = null;
        spawnTime = 0f;
        
        // Stop any running coroutines
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
        
        Debug.Log("🔄 Bullet reset for pool return");
    }
}
