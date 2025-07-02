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
        
        // Update UI display với skins hiện tại
        UpdateSkinDisplay();
    }

    void EnsureGameManagerReady()
    {
        if (GameManager.Instance != null)
        {
            // Force refresh default skins để đảm bảo có skins
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
            UpdateSkinDisplay(); // Update UI sau khi random
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
            Debug.LogWarning("[SkinManager] GameManager not ready, cannot update skin display");
            return;
        }
        
        // Get current skins từ GameManager
        Sprite player1Skin = GameManager.Instance.GetPlayer1Skin();
        Sprite player2Skin = GameManager.Instance.GetPlayer2Skin();
        
        // Update UI display với current skins từ GameManager
        if (player1SkinDisplay != null)
        {
            if (player1Skin != null)
            {
                player1SkinDisplay.sprite = player1Skin;
                // Đảm bảo image hiển thị sprite thay vì màu trắng
                player1SkinDisplay.color = Color.white;
            }
            else
            {
                Debug.LogWarning("[SkinManager] Player 1 skin is null!");
            }
        }
        
        if (player2SkinDisplay != null)
        {
            if (player2Skin != null)
            {
                player2SkinDisplay.sprite = player2Skin;
                // Đảm bảo image hiển thị sprite thay vì màu trắng
                player2SkinDisplay.color = Color.white;
            }
            else
            {
                Debug.LogWarning("[SkinManager] Player 2 skin is null!");
            }
        }

        Debug.Log($"[SkinManager] UI Updated - P1: {player1Skin?.name ?? "null"}, P2: {player2Skin?.name ?? "null"}");
    }

    // Thêm method để manual update (có thể gọi từ button để test)
    public void ForceUpdateDisplay()
    {
        EnsureGameManagerReady();
        UpdateSkinDisplay();
    }

    // Method để set skin manually cho từng player
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
