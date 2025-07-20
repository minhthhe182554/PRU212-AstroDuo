using UnityEngine;

public class WindBehaviour : MonoBehaviour
{
    [Header("Wind Settings")]
    [SerializeField] private float maxWindStrength = 5f; // Tốc độ gió tối đa
    [SerializeField] private Vector2 windDirection; // Hướng gió (normalized)
    [SerializeField] private float windStrength; // Cường độ gió hiện tại
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer windIndicator; // Sprite hiển thị hướng gió
    [SerializeField] private float minScale = 0.5f; // Scale tối thiểu cho arrow
    [SerializeField] private float maxScale = 2.0f; // Scale tối đa cho arrow
    
    public static WindBehaviour Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        // Random hướng gió khi map bắt đầu
        GenerateRandomWind();
        
        // Tự động tìm SpriteRenderer nếu chưa gán
        if (windIndicator == null)
            windIndicator = GetComponent<SpriteRenderer>();
        
        // Cập nhật hướng và scale sprite theo wind
        UpdateWindVisual();
        
        Debug.Log($"🌪️ Wind generated - Direction: {windDirection}, Strength: {windStrength}");
    }
    
    private void GenerateRandomWind()
    {
        // Random hướng gió (8 hướng chính: N, NE, E, SE, S, SW, W, NW)
        float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
        float randomAngle = angles[Random.Range(0, angles.Length)];
        
        // Convert angle to direction vector
        float radians = randomAngle * Mathf.Deg2Rad;
        windDirection = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)).normalized;
        
        // Random wind strength (1 -> maxWindStrength)
        windStrength = Random.Range(1f, maxWindStrength + 0.1f);
    }
    
    private void UpdateWindVisual()
    {
        if (windIndicator != null)
        {
            // Xoay sprite theo hướng gió - FIX cho sprite hướng phải ban đầu
            float angle = Mathf.Atan2(windDirection.y, windDirection.x) * Mathf.Rad2Deg;
            windIndicator.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // Scale chiều Y theo wind strength
            float scaleY = Mathf.Lerp(minScale, maxScale, windStrength / maxWindStrength);
            Vector3 newScale = windIndicator.transform.localScale;
            newScale.y = scaleY;
            windIndicator.transform.localScale = newScale;
            
            Debug.Log($"🎯 Arrow scaled - Wind Strength: {windStrength:F1}, Scale Y: {scaleY:F2}");
        }
    }
    
    /// <summary>
    /// Tính toán wind effect cho jet dựa trên hướng di chuyển của jet
    /// </summary>
    /// <param name="jetDirection">Hướng di chuyển của jet (normalized)</param>
    /// <returns>Wind effect (âm = ngược gió/vuông góc, dương = xuôi gió)</returns>
    public float GetWindEffect(Vector2 jetDirection)
    {
        if (jetDirection.magnitude == 0) return 0f;
        
        // Tính dot product để xác định góc giữa hướng jet và hướng gió
        float dotProduct = Vector2.Dot(jetDirection.normalized, windDirection);
        
        if (dotProduct >= 0.707f) // 0-45 độ: Cùng hướng hoặc gần cùng hướng
        {
            // Bay cùng hướng gió → bonus full speed
            return windStrength;
        }
        else if (dotProduct >= 0f) // 45-90 độ: Hơi lệch hướng gió
        {
            // Bay hơi lệch hướng gió → bonus 1/2 speed
            return windStrength * 0.5f;
        }
        else // 90-180 độ: Vuông góc hoặc ngược hướng gió
        {
            // Bay vuông góc hoặc ngược gió → penalty 1/2 speed
            return -windStrength * 0.5f;
        }
    }
    
    /// <summary>
    /// Lấy thông tin gió để hiển thị UI
    /// </summary>
    public string GetWindInfo()
    {
        string directionText = GetDirectionText();
        return $"Wind: {directionText} {windStrength:F1}";
    }
    
    private string GetDirectionText()
    {
        float angle = Mathf.Atan2(windDirection.x, windDirection.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        
        if (angle >= 337.5f || angle < 22.5f) return "N";
        else if (angle >= 22.5f && angle < 67.5f) return "NE";
        else if (angle >= 67.5f && angle < 112.5f) return "E";
        else if (angle >= 112.5f && angle < 157.5f) return "SE";
        else if (angle >= 157.5f && angle < 202.5f) return "S";
        else if (angle >= 202.5f && angle < 247.5f) return "SW";
        else if (angle >= 247.5f && angle < 292.5f) return "W";
        else return "NW";
    }
    
    // Method để cập nhật lại wind (có thể gọi từ UI debug)
    public void RegenerateWind()
    {
        GenerateRandomWind();
        UpdateWindVisual();
        Debug.Log($"�� Wind regenerated - Direction: {windDirection}, Strength: {windStrength}");
    }
    
    // Getter methods
    public Vector2 GetWindDirection() => windDirection;
    public float GetWindStrength() => windStrength;
    public float GetCurrentScaleY() => windIndicator != null ? windIndicator.transform.localScale.y : 1f;
    
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Vẽ hướng gió trong editor
            Gizmos.color = Color.cyan;
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)(windDirection * 2f);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.1f);
            
            // Vẽ text hiển thị wind strength
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, 
                $"Wind: {windStrength:F1}");
            #endif
        }
    }
}
