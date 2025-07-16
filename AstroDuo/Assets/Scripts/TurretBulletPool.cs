using System.Collections.Generic;
using UnityEngine;

public class TurretBulletPool : MonoBehaviour
{
    public static TurretBulletPool Instance { get; private set; }

    [Header("Turret Bullet Pool")]
    [SerializeField] private GameObject turretBulletPrefab;
    [SerializeField] private int poolSize = 10; // More bullets for continuous firing
    [SerializeField] private Transform poolParent;

    private Queue<GameObject> bulletPool;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePool()
    {
        bulletPool = new Queue<GameObject>();

        if (poolParent == null)
        {
            GameObject poolParentObj = new GameObject("TurretBulletPool");
            poolParent = poolParentObj.transform;
            poolParent.SetParent(transform);
        }

        if (turretBulletPrefab == null)
        {
            CreateDefaultTurretBulletPrefab();
        }

        // Pre-populate pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(turretBulletPrefab, poolParent);
            bullet.name = $"TurretBullet_{i+1}";
            bullet.tag = "TurretBullet";
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
        
        Debug.Log($"🎯 Initialized turret bullet pool with {poolSize} bullets");
    }

    private void CreateDefaultTurretBulletPrefab()
    {
        turretBulletPrefab = new GameObject("TurretBullet");

        // Sprite - make it red to distinguish from player bullets
        SpriteRenderer sr = turretBulletPrefab.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRedSprite();
        sr.sortingOrder = 10;
        turretBulletPrefab.transform.localScale = new Vector3(0.3f, 0.3f, 1f); // Slightly bigger

        // Physics
        BoxCollider2D collider = turretBulletPrefab.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D rb = turretBulletPrefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // Behavior
        turretBulletPrefab.AddComponent<TurretBulletBehaviour>();
        turretBulletPrefab.SetActive(false);
    }

    private Sprite CreateRedSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.red); // Red color for turret bullets
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
    }

    public GameObject GetTurretBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        else
        {
            // Pool exhausted, create new
            GameObject bullet = Instantiate(turretBulletPrefab, poolParent);
            bullet.tag = "TurretBullet";
            return bullet;
        }
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.SetParent(poolParent);

        // Reset properties
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset bullet behaviour
        TurretBulletBehaviour bulletBehaviour = bullet.GetComponent<TurretBulletBehaviour>();
        if (bulletBehaviour != null)
        {
            bulletBehaviour.ResetBullet();
        }

        bulletPool.Enqueue(bullet);
    }

    public int GetAvailableBullets()
    {
        return bulletPool.Count;
    }
}