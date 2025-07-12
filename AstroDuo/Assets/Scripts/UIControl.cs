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
        SceneManager.LoadScene(GameConst.SAMPLE_SKIN_SCENE);
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
}
