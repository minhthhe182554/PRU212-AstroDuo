using UnityEngine;

public class SaberCollisionHandler : MonoBehaviour 
{
    [Header("Saber Stats")]
    [SerializeField] private int maxHP = 2;
    private int currentHP;
    private int ownerPlayerId = -1;
    private JetsBehaviour ownerJet;
    
    public void Initialize(int playerId, JetsBehaviour jet)
    {
        ownerPlayerId = playerId;
        ownerJet = jet;
        currentHP = maxHP;
        Debug.Log($"⚔️ Saber initialized for Player {playerId} with {maxHP} HP");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"⚔️ Saber hit: {other.name} | Tag: {other.tag} | HP: {currentHP}");
        
        if (other.CompareTag("CanDestroyBlock"))
        {
            HandleBlockDestruction(other);
        }
        else if (other.CompareTag("Jet"))
        {
            HandleJetCollision(other);
        }
        // Shield và CannotDestroyBlock sẽ được ignore thông qua physics layer matrix
    }
    
    private void HandleBlockDestruction(Collider2D blockCollider)
    {
        UnityEngine.Tilemaps.Tilemap tilemap = blockCollider.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        if (tilemap != null)
        {
            // Destroy block
            Vector3 saberPosition = transform.position;
            Vector3Int cellPosition = tilemap.WorldToCell(saberPosition);
            
            // Try to find and destroy tile
            bool tileDestroyed = DestroyTileAtPosition(tilemap, saberPosition);
            
            if (tileDestroyed)
            {
                // Giảm HP khi phá block
                TakeDamage(1);
                Debug.Log($"⚔️ SABER DESTROYED BLOCK! HP: {currentHP}/{maxHP}");
            }
        }
    }
    
    private bool DestroyTileAtPosition(UnityEngine.Tilemaps.Tilemap tilemap, Vector3 worldPosition)
    {
        // Similar logic như bullet destruction
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
            UnityEngine.Tilemaps.TileBase tileAtPosition = tilemap.GetTile(tilePosition);
            
            if (tileAtPosition != null)
            {
                tilemap.SetTile(tilePosition, null);
                Debug.Log($"✅ Saber destroyed tile at {tilePosition}");
                return true;
            }
        }
        
        return false;
    }
    
    private void HandleJetCollision(Collider2D jetCollider)
    {
        JetsBehaviour hitJet = jetCollider.GetComponent<JetsBehaviour>();
        if (hitJet != null && hitJet.playerId != ownerPlayerId)
        {
            // Saber collision với enemy jet = instant kill (-2 HP)
            TakeDamage(2);
            
            // Award point to saber owner
            if (GameManager.Instance != null)
            {
                if (ownerPlayerId == 1)
                {
                    GameManager.Instance.AddPlayer1Score();
                }
                else if (ownerPlayerId == 2)
                {
                    GameManager.Instance.AddPlayer2Score();
                }
                
                Debug.Log($"⚔️ SABER KILL! Player {ownerPlayerId} destroyed Player {hitJet.playerId} (penetrated shield!)");
                
                // Load score scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(GameConst.SCORE_SCENE);
            }
        }
    }
    
    private void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"⚔️ Saber took {damage} damage! HP: {currentHP}/{maxHP}");
        
        if (currentHP <= 0)
        {
            DeactivateSaber();
        }
    }
    
    private void DeactivateSaber()
    {
        Debug.Log("⚔️ SABER DESTROYED! HP reached 0");
        
        // Disable both sabers
        if (ownerJet != null)
        {
            Transform leftSaber = ownerJet.transform.Find("LeftSaber");
            Transform rightSaber = ownerJet.transform.Find("RightSaber");
            
            if (leftSaber != null) leftSaber.gameObject.SetActive(false);
            if (rightSaber != null) rightSaber.gameObject.SetActive(false);
            
            // Reset jet speed
            ownerJet.ResetSpeedBoost();
        }
    }
    
    public int GetCurrentHP()
    {
        return currentHP;
    }
} 