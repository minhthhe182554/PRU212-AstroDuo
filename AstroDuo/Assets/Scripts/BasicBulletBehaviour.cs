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
        spawnTime = Time.time; // NEW: Record spawn time
        
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
        Debug.Log($"🎯 Bullet owner set: Player {playerId} ({playerName})");
    }
    
    // NEW: Check if should ignore collision with specific object
    private bool ShouldIgnoreCollision(GameObject hitObject)
    {
        // Ignore collision with owner for a short time after spawn
        if (ownerGameObject != null && hitObject == ownerGameObject)
        {
            float timeSinceSpawn = Time.time - spawnTime;
            if (timeSinceSpawn < ignoreOwnerDuration)
            {
                Debug.Log($"🚫 Ignoring collision with owner (time: {timeSinceSpawn:F2}s)");
                return true;
            }
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
    }
    
    // Handle collision with Jet players
    private void HandleJetCollision(Collider2D jetCollider)
    {
        // Get hit player's ID from their name or component
        int hitPlayerId = GetPlayerIdFromJet(jetCollider.gameObject);
        
        Debug.Log($"💥 Bullet (Owner: Player {ownerId}) hit Player {hitPlayerId} ({jetCollider.name})");
        
        // Check if bullet hit a different player (not self)
        if (ownerId != -1 && hitPlayerId != -1 && ownerId != hitPlayerId)
        {
            // Bullet owner gets a point
            AddScoreToPlayer(ownerId);
            Debug.Log($"⭐ Player {ownerId} scored! Hit Player {hitPlayerId}");
            
            // Check if game should end
            CheckGameEnd();
        }
        else if (ownerId == hitPlayerId)
        {
            // NEW: Self-shot - subtract score if player has points
            SubtractScoreFromPlayer(ownerId);
            
            // Still check if game should end (in case we want to handle end conditions)
            CheckGameEnd();
        }
        else
        {
            Debug.LogWarning($"❓ Unknown players in collision: Owner={ownerId}, Hit={hitPlayerId}");
        }
        
        ReturnToPool();
    }
    
    // Get player ID from GameObject name or component
    private int GetPlayerIdFromJet(GameObject jet)
    {
        // First try to get from JetsBehaviour component (more reliable)
        JetsBehaviour jetBehaviour = jet.GetComponent<JetsBehaviour>();
        if (jetBehaviour != null)
        {
            return jetBehaviour.playerId;
        }
        
        // Fallback to name pattern matching
        string jetName = jet.name.ToLower();
        if (jetName.Contains("player1") || jetName.Contains("p1") || jetName.Contains("jet1"))
        {
            return 1;
        }
        else if (jetName.Contains("player2") || jetName.Contains("p2") || jetName.Contains("jet2"))
        {
            return 2;
        }
        
        Debug.LogWarning($"❓ Could not determine player ID for jet: {jetName}");
        return -1;
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
            GameManager.Instance.Player1Score++;
            Debug.Log($"📊 Player 1 Score: {GameManager.Instance.Player1Score}");
        }
        else if (playerId == 2)
        {
            GameManager.Instance.Player2Score++;
            Debug.Log($"📊 Player 2 Score: {GameManager.Instance.Player2Score}");
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
            if (GameManager.Instance.Player1Score > 0)
            {
                GameManager.Instance.Player1Score--;
                Debug.Log($"🤡 Player 1 shot themselves! Score reduced to: {GameManager.Instance.Player1Score}");
            }
            else
            {
                Debug.Log($"🤷 Player 1 shot themselves but score is already 0! No penalty.");
            }
        }
        else if (playerId == 2)
        {
            if (GameManager.Instance.Player2Score > 0)
            {
                GameManager.Instance.Player2Score--;
                Debug.Log($"🤡 Player 2 shot themselves! Score reduced to: {GameManager.Instance.Player2Score}");
            }
            else
            {
                Debug.Log($"🤷 Player 2 shot themselves but score is already 0! No penalty.");
            }
        }
    }
    
    // Check if game should end and switch to ScoreScene
    private void CheckGameEnd()
    {
        if (GameManager.Instance == null) return;
        
        int player1Score = GameManager.Instance.Player1Score;
        int player2Score = GameManager.Instance.Player2Score;
        
        // Switch to ScoreScene immediately for testing
        // Later you can add win conditions like: if (player1Score >= GameConst.MAX_SCORE || player2Score >= GameConst.MAX_SCORE)
        Debug.Log($"🏁 Switching to Score Scene! P1: {player1Score}, P2: {player2Score}");
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
    
    private void ReturnToPool()
    {
        // Reset owner when returning to pool
        ownerId = -1;
        ownerName = "";
        ownerGameObject = null; // NEW: Reset owner reference
        
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
