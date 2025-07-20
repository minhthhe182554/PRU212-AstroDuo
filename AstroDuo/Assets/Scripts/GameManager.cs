using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public static GameManager Instance;

    [Header("Score")]
    public int Player1Score { get; set; } = 0;
    public int Player2Score { get; set; } = 0;
    
    [Header("Previous Score (for ScoreScene animation)")]
    public int PreviousPlayer1Score { get; private set; } = 0;
    public int PreviousPlayer2Score { get; private set; } = 0;

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

    [Header("Map Management")]
    public string CurrentMap { get; private set; }

    [Header("Turret Penalty Tracking")]
    public bool Player1TurretPenalty { get; private set; } = false;
    public bool Player2TurretPenalty { get; private set; } = false;

    [Header("Spin Animation Settings")]
    [SerializeField] private float spinSpeed = 720f; // Degrees per second
    [SerializeField] private float spinDuration = 0.5f; // Half second spin

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

    // NEW: Methods to update scores while tracking previous values
    public void AddPlayer1Score()
    {
        PreviousPlayer1Score = Player1Score;
        Player1Score++;
        Debug.Log($"📊 Player 1 Score: {PreviousPlayer1Score} → {Player1Score}");
    }

    public void AddPlayer2Score()
    {
        PreviousPlayer2Score = Player2Score;
        Player2Score++;
        Debug.Log($"📊 Player 2 Score: {PreviousPlayer2Score} → {Player2Score}");
    }

    public void SubtractPlayer1Score()
    {
        if (Player1Score > 0)
        {
            PreviousPlayer1Score = Player1Score;
            Player1Score--;
            Debug.Log($"🤡 Player 1 shot themselves! Score: {PreviousPlayer1Score} → {Player1Score}");
        }
        else
        {
            Debug.Log($"🤷 Player 1 shot themselves but score is already 0! No penalty.");
        }
    }

    public void SubtractPlayer2Score()
    {
        if (Player2Score > 0)
        {
            PreviousPlayer2Score = Player2Score;
            Player2Score--;
            Debug.Log($"🤡 Player 2 shot themselves! Score: {PreviousPlayer2Score} → {Player2Score}");
        }
        else
        {
            Debug.Log($"🤷 Player 2 shot themselves but score is already 0! No penalty.");
        }
    }

    // Thêm method để force refresh skins (useful cho debugging)
    public void RefreshDefaultSkins()
    {
        InitializeDefaultSkins();
    }

    // Map Management Methods
    public string GetRandomMap()
    {
        if (GameConst.AVAILABLE_MAPS.Length == 0)
        {
            Debug.LogError("No available maps configured!");
            return GameConst.SAMPLE_SCENE;
        }
        
        int randomIndex = Random.Range(0, GameConst.AVAILABLE_MAPS.Length);
        string selectedMap = GameConst.AVAILABLE_MAPS[randomIndex];
        CurrentMap = selectedMap;
        
        Debug.Log($"🎯 Selected random map: {selectedMap}");
        return selectedMap;
    }

    public void SetCurrentMap(string mapName)
    {
        CurrentMap = mapName;
        Debug.Log($"📍 Current map set to: {mapName}");
    }

    // Check if any player has won
    public bool HasWinner()
    {
        return Player1Score >= GameConst.MAX_SCORE || Player2Score >= GameConst.MAX_SCORE;
    }

    public int GetWinnerPlayerId()
    {
        if (Player1Score >= GameConst.MAX_SCORE)
            return 1;
        else if (Player2Score >= GameConst.MAX_SCORE)
            return 2;
        else
            return -1; // No winner yet
    }

    // Reset scores for new game session
    public void ResetScores()
    {
        Player1Score = 0;
        Player2Score = 0;
        PreviousPlayer1Score = 0;
        PreviousPlayer2Score = 0;
        ClearTurretPenaltyFlags(); // Clear penalty flags
        
        // NEW: Force reset reverse timer when starting new game
        JetsBehaviour.ForceResetReverseTimer();
        
        Debug.Log("🔄 Scores reset for new game session");
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

    // Add getter for available skins (for SkinManager)
    public Sprite[] GetAvailableSkins()
    {
        return availableSkins;
    }

    // RESTORED: Getter methods that SkinManager needs
    public Sprite GetPlayer1Skin()
    {
        return Player1CurrentSkin;
    }

    public Sprite GetPlayer2Skin()
    {
        return Player2CurrentSkin;
    }

    // NEW: Turret penalty methods - track for animation
    public void TurretPenaltyPlayer1()
    {
        PreviousPlayer1Score = Player1Score;
        Player1TurretPenalty = true; // Flag for spin animation
        
        if (Player1Score > 0)
        {
            Player1Score--;
            Debug.Log($"💔 [TURRET PENALTY] Player 1 lost 1 point: {PreviousPlayer1Score} → {Player1Score}");
        }
        else
        {
            Debug.Log($"💔 [TURRET PENALTY] Player 1 at 0 points - will trigger spin animation in ScoreScene!");
        }
    }

    public void TurretPenaltyPlayer2()
    {
        PreviousPlayer2Score = Player2Score;
        Player2TurretPenalty = true; // Flag for spin animation
        
        if (Player2Score > 0)
        {
            Player2Score--;
            Debug.Log($"💔 [TURRET PENALTY] Player 2 lost 1 point: {PreviousPlayer2Score} → {Player2Score}");
        }
        else
        {
            Debug.Log($"💔 [TURRET PENALTY] Player 2 at 0 points - will trigger spin animation in ScoreScene!");
        }
    }

    // NEW: Check if game should end after turret penalty
    public bool ShouldEndGameAfterTurretPenalty()
    {
        return true; // Always end game after turret penalty
    }

    // NEW: Clear penalty flags (called when entering new map)
    public void ClearTurretPenaltyFlags()
    {
        Player1TurretPenalty = false;
        Player2TurretPenalty = false;
        Debug.Log("🔄 Turret penalty flags cleared");
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
