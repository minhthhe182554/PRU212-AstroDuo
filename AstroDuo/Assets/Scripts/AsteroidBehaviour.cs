using UnityEngine;
using System;

public class AsteroidBehaviour : MonoBehaviour 
{
    [Header("Movement")]
    [SerializeField] private float driftSpeed = 1f;
    [SerializeField] private float rotationSpeed = 30f;
    
    [Header("Destruction")]
    [SerializeField] private float lifeTime = 30f; // Tự hủy sau 30 giây
    
    public event Action OnAsteroidDestroyed;
    
    private Vector2 driftDirection;
    
    void Start()
    {
        // Random hướng drift
        driftDirection = UnityEngine.Random.insideUnitCircle.normalized;
        
        // Tự hủy sau một thời gian
        Destroy(gameObject, lifeTime);
    }
    
    void Update()
    {
        // Di chuyển chậm
        transform.Translate(driftDirection * driftSpeed * Time.deltaTime, Space.World);
        
        // Xoay chậm
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    void OnDestroy()
    {
        OnAsteroidDestroyed?.Invoke();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Có thể add logic va chạm với player ở đây
        if (other.CompareTag("Player"))
        {
            Debug.Log("💥 Player hit asteroid!");
            // TODO: Add damage logic
        }
    }
} 