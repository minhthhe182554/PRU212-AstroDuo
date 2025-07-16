using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [Header("Player Bullet Pools")]
    [SerializeField] private GameObject basicBulletPrefab;
    [SerializeField] private int poolSizePerPlayer = 3; // 3 bullets per player
    [SerializeField] private Transform poolParent;

    // Separate pools for each player
    private Dictionary<int, Queue<GameObject>> playerBulletPools;
    private Dictionary<int, Transform> playerPoolParents;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePools()
    {
        playerBulletPools = new Dictionary<int, Queue<GameObject>>();
        playerPoolParents = new Dictionary<int, Transform>();

        if (poolParent == null)
        {
            GameObject poolParentObj = new GameObject("BulletPools");
            poolParent = poolParentObj.transform;
            poolParent.SetParent(transform);
        }

        if (basicBulletPrefab == null)
        {
            CreateDefaultBulletPrefab();
        }

        // Initialize pools for Player 1 and Player 2
        InitializePlayerPool(1);
        InitializePlayerPool(2);
    }

    private void InitializePlayerPool(int playerId)
    {
        // Create player-specific pool parent
        GameObject playerPoolParentObj = new GameObject($"Player{playerId}_BulletPool");
        Transform playerPoolParent = playerPoolParentObj.transform;
        playerPoolParent.SetParent(poolParent);
        playerPoolParents[playerId] = playerPoolParent;

        // Create player-specific bullet pool
        Queue<GameObject> bulletPool = new Queue<GameObject>();
        
        // Pre-populate pool with 3 bullets for this player
        for (int i = 0; i < poolSizePerPlayer; i++)
        {
            GameObject bullet = Instantiate(basicBulletPrefab, playerPoolParent);
            bullet.name = $"Player{playerId}_Bullet_{i+1}";
            bullet.tag = $"Player{playerId}Bullet"; // Set player-specific tag
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
        
        playerBulletPools[playerId] = bulletPool;
        Debug.Log($"🔫 Initialized bullet pool for Player {playerId} with {poolSizePerPlayer} bullets");
    }

    private void CreateDefaultBulletPrefab()
    {
        basicBulletPrefab = new GameObject("BasicBullet");

        // Sprite
        SpriteRenderer sr = basicBulletPrefab.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.sortingOrder = 10;
        basicBulletPrefab.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        // Physics
        BoxCollider2D collider = basicBulletPrefab.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D rb = basicBulletPrefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // Behavior
        basicBulletPrefab.AddComponent<BasicBulletBehaviour>();
        basicBulletPrefab.SetActive(false);
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
    }

    public GameObject GetBasicBullet(int playerId)
    {
        if (!playerBulletPools.ContainsKey(playerId))
        {
            Debug.LogError($"❌ No bullet pool found for Player {playerId}!");
            return null;
        }

        Queue<GameObject> bulletPool = playerBulletPools[playerId];
        
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            Debug.Log($"🎯 Player {playerId} got bullet from pool. Remaining in pool: {bulletPool.Count}");
            return bullet;
        }
        else
        {
            Debug.LogWarning($"⚠️ Player {playerId} bullet pool exhausted! No bullets available.");
            return null; // Don't create new bullets when pool is exhausted
        }
    }

    public void ReturnBullet(GameObject bullet, int playerId)
    {
        if (!playerBulletPools.ContainsKey(playerId))
        {
            Debug.LogError($"❌ No bullet pool found for Player {playerId} when returning bullet!");
            Destroy(bullet);
            return;
        }

        bullet.SetActive(false);
        bullet.transform.SetParent(playerPoolParents[playerId]);

        // Reset properties
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset bullet behaviour
        BasicBulletBehaviour bulletBehaviour = bullet.GetComponent<BasicBulletBehaviour>();
        if (bulletBehaviour != null)
        {
            bulletBehaviour.ResetBullet();
        }

        playerBulletPools[playerId].Enqueue(bullet);
        Debug.Log($"🔄 Player {playerId} bullet returned to pool. Pool size: {playerBulletPools[playerId].Count}");
    }

    // Legacy method for backward compatibility - will try to determine player from bullet tag
    public void ReturnBullet(GameObject bullet)
    {
        int playerId = GetPlayerIdFromBulletTag(bullet.tag);
        if (playerId != -1)
        {
            ReturnBullet(bullet, playerId);
        }
        else
        {
            Debug.LogError($"❌ Cannot determine player ID from bullet tag: {bullet.tag}");
            Destroy(bullet);
        }
    }

    private int GetPlayerIdFromBulletTag(string tag)
    {
        if (tag == "Player1Bullet") return 1;
        if (tag == "Player2Bullet") return 2;
        return -1;
    }

    public int GetAvailableBullets(int playerId)
    {
        if (playerBulletPools.ContainsKey(playerId))
        {
            return playerBulletPools[playerId].Count;
        }
        return 0;
    }
}
