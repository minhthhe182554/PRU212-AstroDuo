using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("Toggle References")]
    [SerializeField] 
    private Toggle shieldSupportToggle;
    [SerializeField] 
    private Toggle fixedSpawnToggle;
    [SerializeField] 
    private Toggle powerUpsToggle;
    [SerializeField] 
    private Toggle soundsToggle;
    [SerializeField] 
    private Toggle startingPowerUps;
    
    [Header("Button References")]
    [SerializeField] 
    private Button saveButton;
    [SerializeField] 
    private Button resetButton;

    void Start()
    {
        // Load current settings from GameManager to UI
        LoadSettingsToUI();
        
        // Set up button listeners
        saveButton.onClick.AddListener(SaveSettings);
        resetButton.onClick.AddListener(ResetToDefault);
    }
    
    private void LoadSettingsToUI()
    {
        if (GameManager.Instance != null)
        {
            shieldSupportToggle.isOn = GameManager.Instance.ShieldSupport;
            fixedSpawnToggle.isOn = GameManager.Instance.FixedSpawn;
            powerUpsToggle.isOn = GameManager.Instance.PowerUps;
            startingPowerUps.isOn = GameManager.Instance.StartingPowerUps;
            soundsToggle.isOn = GameManager.Instance.Sounds;
        }
    }
    
    private void SaveSettings()
    {
        // Play button-click sound here
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateSettings(
                shieldSupportToggle.isOn,
                fixedSpawnToggle.isOn,
                powerUpsToggle.isOn,
                startingPowerUps.isOn,
                soundsToggle.isOn
            );
            
            Debug.Log("Settings saved!");

            GoBackToMainScene();
        }
    }
    
    private void ResetToDefault()
    {
        // Play button-click sound here
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetSettingsToDefault();
            LoadSettingsToUI(); 
        }
    }

    public void GoBackToMainScene()
    {
       SceneManager.LoadScene(GameConst.MAIN_SCENE);
    }
}
