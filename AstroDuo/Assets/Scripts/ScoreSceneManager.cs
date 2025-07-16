using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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
    
    [Header("Auto Transition Settings")]
    [SerializeField] private float scoreDisplayTime = 10f; // Time to show scores before next map
    
    [Header("Spin Animation Settings")]
    [SerializeField] private float spinSpeed = 720f; // Degrees per second
    [SerializeField] private float spinDuration = 0.5f; // Half second spin
    
    // Base positions (0-0 score positions)
    private Vector3 player1BasePosition;
    private Vector3 player2BasePosition;
    
    void Start()
    {
        InitializeReferences();
        StoreBasePositions();
        UpdatePlayerPositions();
        
        // Start auto-transition logic
        StartCoroutine(HandleAutoTransition());
    }
    
    // Handle automatic transition to next map or winner scene
    private IEnumerator HandleAutoTransition()
    {
        // Wait for the specified time
        yield return new WaitForSeconds(scoreDisplayTime);
        
        // Clear turret penalty flags before transitioning
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearTurretPenaltyFlags();
        }
        
        // Check if someone has won
        if (GameManager.Instance != null && GameManager.Instance.HasWinner())
        {
            int winnerId = GameManager.Instance.GetWinnerPlayerId();
            Debug.Log($"🏆 Player {winnerId} wins! Going to Winner Scene");
            SceneManager.LoadScene(GameConst.WINNER_SCENE);
        }
        else
        {
            // No winner yet, continue to next random map
            if (GameManager.Instance != null)
            {
                string nextMap = GameManager.Instance.GetRandomMap();
                Debug.Log($"🎮 Loading next map: {nextMap}");
                SceneManager.LoadScene(nextMap);
            }
            else
            {
                Debug.LogError("GameManager not found! Loading default scene.");
                SceneManager.LoadScene(GameConst.SAMPLE_SCENE);
            }
        }
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
        
        // Get current and previous scores
        int player1CurrentScore = GameManager.Instance.Player1Score;
        int player2CurrentScore = GameManager.Instance.Player2Score;
        int player1PreviousScore = GameManager.Instance.PreviousPlayer1Score;
        int player2PreviousScore = GameManager.Instance.PreviousPlayer2Score;
        
        // Check for turret penalties
        bool player1TurretPenalty = GameManager.Instance.Player1TurretPenalty;
        bool player2TurretPenalty = GameManager.Instance.Player2TurretPenalty;
        
        Debug.Log($"📊 Score animation: P1: {player1PreviousScore}→{player1CurrentScore} (Turret: {player1TurretPenalty}), P2: {player2PreviousScore}→{player2CurrentScore} (Turret: {player2TurretPenalty})");
        
        // Update Player 1 position/animation
        if (player1Sprite != null)
        {
            if (player1TurretPenalty)
            {
                // Turret penalty: spin animation instead of movement
                Vector3 targetPos = player1BasePosition + Vector3.right * (player1CurrentScore * moveDistance);
                player1Sprite.position = targetPos; // Set position immediately
                StartCoroutine(SpinAnimation(player1Sprite)); // Add spin animation
                Debug.Log($"🌪️ Player 1 turret penalty - performing spin animation!");
            }
            else
            {
                // Normal movement animation
                Vector3 startPos = player1BasePosition + Vector3.right * (player1PreviousScore * moveDistance);
                Vector3 targetPos = player1BasePosition + Vector3.right * (player1CurrentScore * moveDistance);
                
                player1Sprite.position = startPos;
                StartCoroutine(MoveToPosition(player1Sprite, targetPos));
            }
        }
        
        // Update Player 2 position/animation
        if (player2Sprite != null)
        {
            if (player2TurretPenalty)
            {
                // Turret penalty: spin animation instead of movement
                Vector3 targetPos = player2BasePosition + Vector3.right * (player2CurrentScore * moveDistance);
                player2Sprite.position = targetPos; // Set position immediately
                StartCoroutine(SpinAnimation(player2Sprite)); // Add spin animation
                Debug.Log($"🌪️ Player 2 turret penalty - performing spin animation!");
            }
            else
            {
                // Normal movement animation
                Vector3 startPos = player2BasePosition + Vector3.right * (player2PreviousScore * moveDistance);
                Vector3 targetPos = player2BasePosition + Vector3.right * (player2CurrentScore * moveDistance);
                
                player2Sprite.position = startPos;
                StartCoroutine(MoveToPosition(player2Sprite, targetPos));
            }
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

    // NEW: Spin animation coroutine
    private IEnumerator SpinAnimation(Transform sprite)
    {
        float elapsed = 0f;
        float totalRotation = spinSpeed * spinDuration;
        Vector3 startRotation = sprite.eulerAngles;
        
        Debug.Log($"🌪️ [SPIN START] Starting spin animation for {sprite.name}");
        
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float rotationThisFrame = spinSpeed * Time.deltaTime;
            
            sprite.Rotate(0, 0, rotationThisFrame);
            
            yield return null;
        }
        
        Debug.Log($"🌪️ [SPIN END] Spin animation completed for {sprite.name}");
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