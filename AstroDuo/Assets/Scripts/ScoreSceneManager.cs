using UnityEngine;
using System.Collections;

public class ScoreSceneManager : MonoBehaviour
{
    [Header("Player Sprites")]
    [SerializeField] private Transform player1Sprite;
    [SerializeField] private Transform player2Sprite;
    [SerializeField] private string player1ObjectName = "Player1";
    [SerializeField] private string player2ObjectName = "Player2";
    
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 5f;     // Khoảng cách di chuyển ngang mỗi điểm
    [SerializeField] private float animationDuration = 1f; // Thời gian animation
    
    // Base positions (0-0 score positions)
    private Vector3 player1BasePosition;
    private Vector3 player2BasePosition;
    
    void Start()
    {
        InitializeReferences();
        StoreBasePositions();
        UpdatePlayerPositions();
    }
    
    void InitializeReferences()
    {
        // Auto-find sprites if not assigned
        if (player1Sprite == null)
        {
            GameObject p1Object = GameObject.Find(player1ObjectName);
            if (p1Object != null)
            {
                player1Sprite = p1Object.transform;
                Debug.Log($"✅ Found Player1 sprite: {p1Object.name}");
            }
        }
        
        if (player2Sprite == null)
        {
            GameObject p2Object = GameObject.Find(player2ObjectName);
            if (p2Object != null)
            {
                player2Sprite = p2Object.transform;
                Debug.Log($"✅ Found Player2 sprite: {p2Object.name}");
            }
        }
    }
    
    void StoreBasePositions()
    {
        // Store current positions as base (0-0 score) positions
        if (player1Sprite != null)
        {
            player1BasePosition = player1Sprite.position;
            Debug.Log($"📍 Player1 base position: {player1BasePosition}");
        }
        
        if (player2Sprite != null)
        {
            player2BasePosition = player2Sprite.position;
            Debug.Log($"📍 Player2 base position: {player2BasePosition}");
        }
    }
    
    void UpdatePlayerPositions()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is null!");
            return;
        }
        
        // Get current scores
        int player1Score = GameManager.Instance.Player1Score;
        int player2Score = GameManager.Instance.Player2Score;
        
        Debug.Log($"📊 Current scores: P1={player1Score}, P2={player2Score}");
        
        // Update Player 1 position (di chuyển về phía phải)
        if (player1Sprite != null)
        {
            Vector3 targetPos = player1BasePosition + Vector3.right * (player1Score * moveDistance);
            StartCoroutine(MoveToPosition(player1Sprite, targetPos));
        }
        
        // Update Player 2 position (di chuyển về phía phải)
        if (player2Sprite != null)
        {
            Vector3 targetPos = player2BasePosition + Vector3.right * (player2Score * moveDistance);
            StartCoroutine(MoveToPosition(player2Sprite, targetPos));
        }
    }
    
    private IEnumerator MoveToPosition(Transform sprite, Vector3 targetPosition)
    {
        Vector3 startPosition = sprite.position;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            
            sprite.position = Vector3.Lerp(startPosition, targetPosition, progress);
            
            yield return null;
        }
        
        // Ensure exact final position
        sprite.position = targetPosition;
    }
    
    // PUBLIC METHOD - Call này để force update nếu cần
    public void RefreshPositions()
    {
        UpdatePlayerPositions();
    }
    
    // DEBUG METHODS
    [ContextMenu("Update Positions")]
    public void TestUpdatePositions()
    {
        UpdatePlayerPositions();
    }
    
    [ContextMenu("Reset to Base")]
    public void ResetToBase()
    {
        if (player1Sprite != null)
        {
            StartCoroutine(MoveToPosition(player1Sprite, player1BasePosition));
        }
        
        if (player2Sprite != null)
        {
            StartCoroutine(MoveToPosition(player2Sprite, player2BasePosition));
        }
    }
} 