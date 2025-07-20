using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReverseUIManager : MonoBehaviour
{
    public static ReverseUIManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private Image reverseImage;
    [SerializeField] private string reverseImageObjectName = "Reverse";
    
    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 2f;
    
    private Coroutine displayCoroutine;
    private bool isDisplaying = false;
    
    void Awake()
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
    
    void Start()
    {
        FindReverseImage();
        
        // Initially hide the image
        if (reverseImage != null)
        {
            reverseImage.gameObject.SetActive(false);
        }
    }
    
    void OnLevelWasLoaded(int level)
    {
        // Find the ReverseImage in the new scene
        FindReverseImage();
    }
    
    private void FindReverseImage()
    {
        // Try to find ReverseImage by name
        GameObject reverseImageObj = GameObject.Find(reverseImageObjectName);
        
        if (reverseImageObj != null)
        {
            reverseImage = reverseImageObj.GetComponent<Image>();
            
            if (reverseImage != null)
            {
                // Initially hide the image
                reverseImage.gameObject.SetActive(false);
                Debug.Log($"✅ Found ReverseImage UI: {reverseImageObj.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ ReverseImage GameObject found but no Image component!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ ReverseImage GameObject '{reverseImageObjectName}' not found in scene!");
        }
    }
    
    public void ShowReverse()
    {
        if (reverseImage == null)
        {
            Debug.LogWarning("⚠️ ReverseImage UI not found! Cannot display image.");
            return;
        }
        
        // Stop any existing display coroutine
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        // Start new display coroutine
        displayCoroutine = StartCoroutine(DisplayReverseCoroutine());
    }
    
    private IEnumerator DisplayReverseCoroutine()
    {
        isDisplaying = true;
        
        // Show the image
        reverseImage.gameObject.SetActive(true);
        
        Debug.Log($"🔄 Displaying REVERSE image for {displayDuration} seconds");
        
        // Wait for the display duration
        yield return new WaitForSeconds(displayDuration);
        
        // Hide the image
        reverseImage.gameObject.SetActive(false);
        
        Debug.Log($"✅ REVERSE image hidden after {displayDuration} seconds");
        
        isDisplaying = false;
        displayCoroutine = null;
    }
    
    public bool IsDisplaying()
    {
        return isDisplaying;
    }
    
    public void SetDisplayDuration(float duration)
    {
        displayDuration = duration;
    }
    
    // NEW: Method to change the reverse image sprite if needed
    public void SetReverseSprite(Sprite newSprite)
    {
        if (reverseImage != null)
        {
            reverseImage.sprite = newSprite;
        }
    }
} 