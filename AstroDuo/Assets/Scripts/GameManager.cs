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

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }   

    void Update()
    {
        
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
