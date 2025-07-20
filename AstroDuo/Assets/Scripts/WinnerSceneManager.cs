using UnityEngine;
using TMPro;

public class WinnerSceneManager : MonoBehaviour
{
    [Header("Winner Display")]
    [SerializeField] private SpriteRenderer winnerSpriteRenderer;
    [SerializeField] private string winnerObjectName = "Winner";
    
    [Header("Player Name Text")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private string playerNameObjectName = "PlayerName";
    
    void Start()
    {
        SetupWinnerDisplay();
    }
    
    void SetupWinnerDisplay()
    {
        // Auto-find winner sprite renderer if not assigned
        if (winnerSpriteRenderer == null)
        {
            GameObject winnerObject = GameObject.Find(winnerObjectName);
            if (winnerObject != null)
            {
                winnerSpriteRenderer = winnerObject.GetComponent<SpriteRenderer>();
                Debug.Log($"✅ Found Winner sprite renderer: {winnerObject.name}");
            }
        }
        
        // Auto-find player name text if not assigned
        if (playerNameText == null)
        {
            GameObject playerNameObject = GameObject.Find(playerNameObjectName);
            if (playerNameObject != null)
            {
                playerNameText = playerNameObject.GetComponent<TextMeshProUGUI>();
                Debug.Log($"✅ Found PlayerName text: {playerNameObject.name}");
            }
        }
        
        if (GameManager.Instance != null)
        {
            int winnerId = GameManager.Instance.GetWinnerPlayerId();
            
            if (winnerId != -1)
            {
                // Apply winner's skin
                Sprite winnerSkin = null;
                
                if (winnerId == 1)
                {
                    winnerSkin = GameManager.Instance.GetPlayer1Skin();
                }
                else if (winnerId == 2)
                {
                    winnerSkin = GameManager.Instance.GetPlayer2Skin();
                }
                
                // Apply skin to sprite renderer
                if (winnerSpriteRenderer != null && winnerSkin != null)
                {
                    winnerSpriteRenderer.sprite = winnerSkin;
                    Debug.Log($"🏆 Applied Player {winnerId} skin to winner display: {winnerSkin.name}");
                }
                else
                {
                    Debug.LogError("Failed to apply winner skin - missing sprite renderer or skin");
                }
                
                // Update player name text
                if (playerNameText != null)
                {
                    playerNameText.text = $"P{winnerId}";
                    Debug.Log($"📝 Updated PlayerName text to: P{winnerId}");
                }
                else
                {
                    Debug.LogError("PlayerName text not found!");
                }
            }
            else
            {
                Debug.LogError("No winner found! This should not happen in WinnerScene.");
            }
        }
        else
        {
            Debug.LogError("GameManager Instance not found!");
        }
    }
}
