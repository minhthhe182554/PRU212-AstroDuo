using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScatterBulletBehaviour : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f; // Ngắn hơn basic bullet
    
    private Coroutine lifetimeCoroutine;
    
    // Track bullet owner (similar to BasicBulletBehaviour)
    public int ownerId = -1;
    public string ownerName = "";
    public GameObject ownerGameObject;
    private float spawnTime;
    
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
    
    public void SetOwner(int playerId, string playerName, GameObject ownerGO)
    {
        ownerId = playerId;
        ownerName = playerName;
        ownerGameObject = ownerGO;
        spawnTime = Time.time;
        Debug.Log($"💥 [SCATTER BULLET OWNER SET] Player {playerId} ({playerName})");
    }
    
    private IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);
        DestroyBullet();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (ownerId == -1)
        {
            Debug.LogWarning($"⚠️ [SCATTER BULLET] Hit {other.name} before owner was set!");
            return;
        }
        
        // IGNORE collision với owner trong thời gian ngắn
        if (ownerGameObject != null && other.gameObject == ownerGameObject)
        {
            float timeSinceSpawn = Time.time - spawnTime;
            if (timeSinceSpawn < 0.1f)
            {
                Debug.Log("💥 Scatter bullet ignored owner collision");
                return;
            }
        }
        
        // ADD DETAILED DEBUG
        Debug.Log($"💥 Scatter bullet hit: {other.name} | Tag: '{other.tag}' | HasTilemap: {other.GetComponent<Tilemap>() != null}");
        
        if (other.CompareTag("CanDestroyBlock"))
        {
            Debug.Log("💥 Handling CanDestroyBlock collision");
            HandleBlockDestruction(other);
        }
        else if (other.CompareTag("CannotDestroyBlock"))
        {
            Debug.Log("💥 Hit indestructible block");
            DestroyBullet();
        }
        else if (other.CompareTag("Jet"))
        {
            Debug.Log("💥 Hit enemy Jet");
            HandleJetCollision(other);
        }
        else if (other.name.Contains("Shield"))
        {
            Debug.Log("💥 Hit Shield - should be blocked");
            HandleShieldCollision(other);
        }
        else
        {
            Debug.Log($"💥 Unknown collision: {other.name} | Tag: {other.tag}");
        }
    }
    
    private void HandleBlockDestruction(Collider2D blockCollider)
    {
        Debug.Log("💥 Attempting to destroy block...");
        
        Tilemap tilemap = blockCollider.GetComponent<Tilemap>();
        if (tilemap != null)
        {
            Vector3 bulletPosition = transform.position;
            Debug.Log($"💥 Tilemap found, bullet position: {bulletPosition}");
            
            if (DestroyTileAtPosition(tilemap, bulletPosition))
            {
                Debug.Log($"💥 SCATTER BULLET DESTROYED BLOCK!");
            }
            else
            {
                Debug.Log($"💥 Failed to destroy tile at {bulletPosition}");
            }
        }
        else
        {
            Debug.Log("💥 No Tilemap component found!");
        }
        DestroyBullet();
    }
    
    private bool DestroyTileAtPosition(Tilemap tilemap, Vector3 worldPosition)
    {
        // Try multiple positions like BasicBullet does
        Vector3[] testPositions = {
            worldPosition,
            worldPosition + Vector3.up * 0.2f,
            worldPosition + Vector3.down * 0.2f,
            worldPosition + Vector3.left * 0.2f,
            worldPosition + Vector3.right * 0.2f
        };
        
        foreach (Vector3 testPos in testPositions)
        {
            Vector3Int tilePosition = tilemap.WorldToCell(testPos);
            TileBase tileAtPosition = tilemap.GetTile(tilePosition);
            
            if (tileAtPosition != null)
            {
                tilemap.SetTile(tilePosition, null);
                Debug.Log($"✅ Scatter bullet destroyed tile at {tilePosition}");
                return true;
            }
        }
        
        Debug.Log($"❌ No tile found around {worldPosition}");
        return false;
    }
    
    private void HandleJetCollision(Collider2D jetCollider)
    {
        JetsBehaviour hitJet = jetCollider.GetComponent<JetsBehaviour>();
        if (hitJet != null && hitJet.playerId != ownerId)
        {
            // Award point to scatter shot owner
            if (GameManager.Instance != null)
            {
                if (ownerId == 1)
                {
                    GameManager.Instance.AddPlayer1Score();
                }
                else if (ownerId == 2)
                {
                    GameManager.Instance.AddPlayer2Score();
                }
                
                Debug.Log($"💥 SCATTER SHOT KILL! Player {ownerId} destroyed Player {hitJet.playerId}");
                SceneManager.LoadScene(GameConst.SCORE_SCENE);
            }
        }
        DestroyBullet();
    }
    
    private void HandleShieldCollision(Collider2D shieldCollider)
    {
        // Check if this is own shield
        Transform shieldParent = shieldCollider.transform.parent;
        if (shieldParent != null && ownerGameObject != null)
        {
            JetsBehaviour shieldOwner = shieldParent.GetComponent<JetsBehaviour>();
            if (shieldOwner != null && shieldOwner.playerId == ownerId)
            {
                Debug.Log($"💥 Scatter bullet ignored own shield (Player {ownerId})");
                return; // Ignore own shield
            }
        }
        
        Debug.Log($"💥 Scatter bullet hit enemy shield - let shield handle blocking");
        
        // DON'T destroy here - let Shield's collision handler do it
        // Shield will call DestroyBullet() through its BlockProjectile() method
    }
    
    public void DestroyBullet()
    {
        // Destroy instead of return to pool (đơn giản hóa)
        Destroy(gameObject);
    }
    
    void OnBecameInvisible()
    {
        DestroyBullet();
    }
} 