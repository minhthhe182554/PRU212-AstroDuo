using UnityEngine;
using System;

public class CollisionForwarder : MonoBehaviour
{
    public Action<Collider2D> OnTriggerEnterCallback;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerEnterCallback?.Invoke(other);
    }
} 