using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControl : MonoBehaviour 
{
    [Header("Scale Settings")]
    [SerializeField] 
    private Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f); // Image scale 1.2x when hover

    public void ToSetting()
    {
        // Play button-click sound here
        SceneManager.LoadScene(GameConst.SETTING_SCENE); 
    }

    public void ToSelectSkin()
    {
        // Play button-click sound here
        SceneManager.LoadScene(GameConst.SELECT_SKIN_SCENE);
    }
    
    public void ToPlayScene()
    {
        // Play button-click sound here
        
        // NEW: Reset scores when starting new game session from SelectSkinScene
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScores();
            
            // Get random map and load it
            string randomMap = GameManager.Instance.GetRandomMap();
            SceneManager.LoadScene(randomMap);
        }
        else
        {
            Debug.LogError("GameManager Instance not found! Loading default scene.");
            SceneManager.LoadScene(GameConst.SAMPLE_SCENE);
        }
    }

    public void OnButtonHoverEnter(RectTransform targetRectTransform)
    {
        if (targetRectTransform != null)
        {
            targetRectTransform.localScale = hoverScale;
        }
    }

    public void OnButtonHoverExit(RectTransform targetRectTransform)
    {
        if (targetRectTransform != null)
        {
            targetRectTransform.localScale = Vector3.one; // return default scale
        }
    }

    public void OnToggleValueChanged()
    {
        //Play toggle on/off sound here
        
        Debug.Log("value changed");
    }

    public void RandomizeSkins()
    {
        // Play button-click sound here
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RandomizeSkins();
            
            // Update UI in SelectSkinScene
            SkinManager skinManager = FindFirstObjectByType<SkinManager>();
            if (skinManager != null)
            {
                skinManager.UpdateSkinDisplay(); 
            }
        }
        else
        {
            Debug.LogError("GameManager Instance not found!");
        }
    }

    // NEW: Play Again method for WinnerScene
    public void PlayAgain()
    {
        // Play button-click sound here
        
        if (GameManager.Instance != null)
        {
            // Reset scores and game state completely
            GameManager.Instance.ResetScores();
            
            // Clear any penalty flags
            GameManager.Instance.ClearTurretPenaltyFlags();
            
            // Optional: Reset skins to default (uncomment if you want this)
            // GameManager.Instance.InitializeDefaultSkins();
            
            Debug.Log("🔄 Game reset! Returning to Main Menu");
            
            // Return to Main Scene
            SceneManager.LoadScene(GameConst.MAIN_SCENE);
        }
        else
        {
            Debug.LogError("GameManager Instance not found! Loading Main Scene anyway.");
            SceneManager.LoadScene(GameConst.MAIN_SCENE);
        }
    }
}
