using UnityEngine;

public class SkinRegister : MonoBehaviour
{

    [Header("Player Sprite Renderers")] 
    [SerializeField] private SpriteRenderer player1Renderer;
    [SerializeField] private SpriteRenderer player2Renderer;
    
    [Header("Auto Find Settings")]
    [SerializeField] private string player1ObjectName = "P1_Jet";
    [SerializeField] private string player2ObjectName = "P2_Jet";
    
    void Start()
    {
        RegisterPlayersWithGameManager();
    }
    
    void RegisterPlayersWithGameManager()
    {

        if (player1Renderer == null)
        {
            GameObject p1Object = GameObject.Find(player1ObjectName);
            if (p1Object != null)
                player1Renderer = p1Object.GetComponent<SpriteRenderer>();
        }

        if (player2Renderer == null)
        {
            GameObject p2Object = GameObject.Find(player2ObjectName);
            if (p2Object != null)
                player2Renderer = p2Object.GetComponent<SpriteRenderer>();
        }

        // Register SpriteRenderers with GameManager
        if (GameManager.Instance != null && player1Renderer != null && player2Renderer != null)
        {
            GameManager.Instance.RegisterPlayerSkinRenderers(player1Renderer, player2Renderer);
        }
        else
        {
            Debug.LogError("Failed to register - missing GameManager or player renderers");
        }
    }
}
