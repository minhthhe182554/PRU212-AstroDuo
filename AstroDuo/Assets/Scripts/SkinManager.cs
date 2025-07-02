using UnityEngine; 
using UnityEngine.UI;

public class SkinManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image player1SkinDisplay; 
    [SerializeField] private Image player2SkinDisplay; 
    [SerializeField] private Button randomSkinsButton; 

    void Start()
    {
        // Set up button listener
        if (randomSkinsButton != null)
        {
            randomSkinsButton.onClick.AddListener(RandomizeSkins);
        }
        
        // Ensure GameManager has default skins
        EnsureGameManagerReady();
        
        // Update UI display with current skin
        UpdateSkinDisplay();
    }

    void EnsureGameManagerReady()
    {
        if (GameManager.Instance != null)
        {
            // Force refresh default skins 
            GameManager.Instance.RefreshDefaultSkins();
        }
        else
        {
            Debug.LogError("[SkinManager] GameManager Instance not found!");
        }
    }

    public void RandomizeSkins()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RandomizeSkins();
            UpdateSkinDisplay(); // Update SelectSkinScene UI after random
        }
        else
        {
            Debug.LogError("GameManager Instance not found!");
        }
    }

    public void UpdateSkinDisplay()
    {
        if (GameManager.Instance == null) 
        {
            Debug.LogWarning("GameManager is null, cannot update skin display");
            return;
        }
        
        // Get current skins 
        Sprite player1Skin = GameManager.Instance.GetPlayer1Skin();
        Sprite player2Skin = GameManager.Instance.GetPlayer2Skin();
        
        // Update UI display with current skins
        if (player1SkinDisplay != null)
        {
            if (player1Skin != null)
            {
                player1SkinDisplay.sprite = player1Skin;
            }
            else
            {
                Debug.LogWarning("Player 1 skin is null!");
            }
        }
        
        if (player2SkinDisplay != null)
        {
            if (player2Skin != null)
            {
                player2SkinDisplay.sprite = player2Skin;
            }
            else
            {
                Debug.LogWarning("Player 2 skin is null!");
            }
        }

        Debug.Log($"UI Updated - P1: {player1Skin?.name ?? "null"}, P2: {player2Skin?.name ?? "null"}");
    }

    // Method to manually set skin to each player
    public void SetPlayer1Skin(int skinIndex)
    {
        if (GameManager.Instance != null)
        {
            Sprite[] availableSkins = GameManager.Instance.GetAvailableSkins();
            if (availableSkins != null && skinIndex >= 0 && skinIndex < availableSkins.Length)
            {
                GameManager.Instance.SetPlayerSkin(1, availableSkins[skinIndex]);
                UpdateSkinDisplay();
            }
        }
    }
    
    public void SetPlayer2Skin(int skinIndex)
    {
        if (GameManager.Instance != null)
        {
            Sprite[] availableSkins = GameManager.Instance.GetAvailableSkins();
            if (availableSkins != null && skinIndex >= 0 && skinIndex < availableSkins.Length)
            {
                GameManager.Instance.SetPlayerSkin(2, availableSkins[skinIndex]);
                UpdateSkinDisplay();
            }
        }
    }

    void Update()
    {
        
    }
}
