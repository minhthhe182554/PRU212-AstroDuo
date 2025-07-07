using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public static GameManager Instance;

    [Header("Score")]
    public int Player1Score { get; set; } = 0;
    public int Player2Score { get; set; } = 0;

    [Header("Game Settings")]
    public bool ShieldSupport { get; set; } = false;
    public bool FixedSpawn { get; set; } = false;
    public bool PowerUps { get; set; } = true;
    public bool StartingPowerUps { get; set; } = false;
    public bool Sounds { get; set; } = true;

    [Header("Player Skin References")]
    public SpriteRenderer Player1Skin { get; set; }
    public SpriteRenderer Player2Skin { get; set; }

    [Header("Available Skins")]
    [SerializeField] private Sprite[] availableSkins; 
    
    [Header("Current Selected Skins")]
    public Sprite Player1CurrentSkin { get; private set; }
    public Sprite Player2CurrentSkin { get; private set; }

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize default skins ngay trong Awake
            InitializeDefaultSkins();
        }
        else
        {
            Destroy(gameObject);
        }
    }   

    void Update()
    {
        
    }

    void InitializeDefaultSkins()
    {
        // Đảm bảo luôn có default skins
        if (availableSkins != null && availableSkins.Length >= 2)
        {
            // Chỉ set default nếu chưa có skin nào được set
            if (Player1CurrentSkin == null)
                Player1CurrentSkin = availableSkins[0];
            
            if (Player2CurrentSkin == null)
                Player2CurrentSkin = availableSkins[1];
        }
        else
        {
            Debug.LogWarning("[GameManager] Available skins not assigned or not enough skins! Please assign at least 2 skins in Inspector.");
        }
    }

    // Thêm method để force refresh skins (useful cho debugging)
    public void RefreshDefaultSkins()
    {
        InitializeDefaultSkins();
    }

    public void RandomizeSkins()
    {
        if (availableSkins == null || availableSkins.Length < 2)
        {
            Debug.LogError("Need at least 2 skins");
            return;
        }

        // Random skin for Player1
        Sprite newPlayer1Skin;
        do
        {
            int randomIndex = Random.Range(0, availableSkins.Length);
            newPlayer1Skin = availableSkins[randomIndex];
        }
        while (newPlayer1Skin == Player1CurrentSkin); // Make sure randomed skin is different to current skin

        // Random skin for Player2 (make sure it different to Player1 skin)
        Sprite newPlayer2Skin;
        do
        {
            int randomIndex = Random.Range(0, availableSkins.Length);
            newPlayer2Skin = availableSkins[randomIndex];
        }
        while (newPlayer2Skin == newPlayer1Skin || newPlayer2Skin == Player2CurrentSkin);

        // Update current skins
        Player1CurrentSkin = newPlayer1Skin;
        Player2CurrentSkin = newPlayer2Skin;

        // Apply to SpriteRenderers if they exist
        ApplySkinsToRenderers();

        Debug.Log($"[GameManager] Skins randomized! Player1: {Player1CurrentSkin.name}, Player2: {Player2CurrentSkin.name}");
    }

    public void ApplySkinsToRenderers()
    {
        if (Player1Skin != null && Player1CurrentSkin != null)
        {
            Player1Skin.sprite = Player1CurrentSkin;
        }

        if (Player2Skin != null && Player2CurrentSkin != null)
        {
            Player2Skin.sprite = Player2CurrentSkin;
        }
    }

    public void SetPlayerSkin(int playerNumber, Sprite newSkin)
    {
        if (newSkin == null)
        {
            Debug.LogError("Skin is null");
            return;
        }

        if (playerNumber == 1)
        {
            // Ensure 2 player has 2 different skins
            if (newSkin != Player2CurrentSkin)
            {
                Player1CurrentSkin = newSkin;
            }
            else
            {
                Debug.LogWarning("Same skins");
                return;
            }
        }
        else if (playerNumber == 2)
        {
            // Ensure 2 player has 2 different skins
            if (newSkin != Player1CurrentSkin)
            {
                Player2CurrentSkin = newSkin;
            }
            else
            {
                Debug.LogWarning("Same skins");
                return;
            }
        }

        ApplySkinsToRenderers();
        Debug.Log($"[GameManager] Player {playerNumber} skin updated to: {newSkin.name}");
    }

    public void RegisterPlayerSkinRenderers(SpriteRenderer player1Renderer, SpriteRenderer player2Renderer)
    {
        Player1Skin = player1Renderer;
        Player2Skin = player2Renderer;
        
        // Apply current skins to Players in other scenes
        ApplySkinsToRenderers();
        
        Debug.Log("[GameManager] Player skin renderers registered and skins applied.");
    }

    // Settings methods
    public void UpdateSettings(bool shieldSupport, bool fixedSpawn, bool powerUps, bool startingPowerUps, bool sounds)
    {
        ShieldSupport = shieldSupport;
        FixedSpawn = fixedSpawn;
        PowerUps = powerUps;
        StartingPowerUps = startingPowerUps;
        Sounds = sounds;
        
        Debug.Log("[GameManager] Settings updated!");
    }

    public void ResetSettingsToDefault()
    {
        ShieldSupport = false;
        FixedSpawn = false;
        PowerUps = true;
        Sounds = true;
        StartingPowerUps = false;

        Debug.Log("[GameManager] Settings reset to default!");
    }
    
    // Getters
    public Sprite[] GetAvailableSkins()
    {
        return availableSkins;
    }

    public Sprite GetPlayer1Skin()
    {
        return Player1CurrentSkin;
    }

    public Sprite GetPlayer2Skin()
    {
        return Player2CurrentSkin;
    }
}

[System.Serializable]
public struct GameSettings
{
    public bool shieldSupport;
    public bool fixedSpawn;
    public bool powerUps;
    public bool startingPowerUps;
    public bool sounds;
}
