using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [Header("Basic Bullet Pool")]
    [SerializeField] private GameObject basicBulletPrefab;
    [SerializeField] private int poolSize = 15;
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
            GameObject poolParentObj = new GameObject("BasicBulletPool");
            poolParent = poolParentObj.transform;
            poolParent.SetParent(transform);
        }

        if (basicBulletPrefab == null)
        {
            CreateDefaultBulletPrefab();
        }

        // Pre-populate pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(basicBulletPrefab, poolParent);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
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

    public GameObject GetBasicBullet()
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
            GameObject bullet = Instantiate(basicBulletPrefab, poolParent);
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

        bulletPool.Enqueue(bullet);
    }
}
