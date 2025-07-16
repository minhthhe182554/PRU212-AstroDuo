using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ReverseUIManager : MonoBehaviour
{
    public static ReverseUIManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI reverseText;
    [SerializeField] private string reverseTextObjectName = "ReverseText";
    
    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private string reverseMessage = "REVERSE";
    
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
        FindReverseText();
        
        // Initially hide the text
        if (reverseText != null)
        {
            reverseText.gameObject.SetActive(false);
        }
    }
    
    void OnLevelWasLoaded(int level)
    {
        // Find the ReverseText in the new scene
        FindReverseText();
    }
    
    private void FindReverseText()
    {
        // Try to find ReverseText by name
        GameObject reverseTextObj = GameObject.Find(reverseTextObjectName);
        
        if (reverseTextObj != null)
        {
            reverseText = reverseTextObj.GetComponent<TextMeshProUGUI>();
            
            if (reverseText != null)
            {
                // Initially hide the text
                reverseText.gameObject.SetActive(false);
                Debug.Log($"✅ Found ReverseText UI: {reverseTextObj.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ ReverseText GameObject found but no TextMeshProUGUI component!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ ReverseText GameObject '{reverseTextObjectName}' not found in scene!");
        }
    }
    
    public void ShowReverse()
    {
        if (reverseText == null)
        {
            Debug.LogWarning("⚠️ ReverseText UI not found! Cannot display message.");
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
        
        // Set the text and show it
        reverseText.text = reverseMessage;
        reverseText.gameObject.SetActive(true);
        
        Debug.Log($"🔄 Displaying REVERSE for {displayDuration} seconds");
        
        // Wait for the display duration
        yield return new WaitForSeconds(displayDuration);
        
        // Hide the text
        reverseText.gameObject.SetActive(false);
        
        Debug.Log($"✅ REVERSE message hidden after {displayDuration} seconds");
        
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
    
    public void SetReverseMessage(string message)
    {
        reverseMessage = message;
    }
} 