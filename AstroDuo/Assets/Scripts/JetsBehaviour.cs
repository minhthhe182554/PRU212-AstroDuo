using System.Collections;
using UnityEngine;

public class JetsBehaviour : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] public float speed = 2f;
    [SerializeField] public float rotationAngle = 20f;
    
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 8f; 
    [SerializeField] private float dashDuration = 0.2f; 
    [SerializeField] private float doubleTapTimeThreshold = 0.3f;
    [SerializeField] private float dashArcAngle = 45f; 
    [SerializeField] private float dashArcIntensity = 1f; 
    [SerializeField] private KeyCode moveKey = KeyCode.LeftArrow; 
    [SerializeField] private KeyCode fireKey = KeyCode.UpArrow; 

    private float lastLeftArrowPressTime;
    private bool isDashing = false;
    
    void Start()
    {
        // Cố định framerate ở 60fps
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // Tắt VSync để đảm bảo targetFrameRate hoạt động
    }

    void Update()
    {
        if (isDashing)
        {
            return; // Cannot change direction while dashing
        }

        AutoMoveForward();

        if (Input.GetKeyDown(moveKey))
        {
            // click 2 times - Dash
            if (Time.time - lastLeftArrowPressTime < doubleTapTimeThreshold)
            {
                StartCoroutine(Dash());
            }
            else
            {
                // If only click 1 time, rotate
                ChangeDirection();
            }

            // update last time clicked left arrow button
            lastLeftArrowPressTime = Time.time;
        }
    }

        // Detect collision với blocks
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("CanDestroyBlock"))
        {
            Debug.Log("Player va chạm với CanDestroyBlock: " + collision.gameObject.name);
        }
        else if (collision.gameObject.CompareTag("CannotDestroyBlock"))
        {
            Debug.Log("Player va chạm với CannotDestroyBlock: " + collision.gameObject.name);
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        float startTime = Time.time;
        
        int rotationSteps = 2;
        bool[] rotationCompleted = new bool[rotationSteps];

        while (Time.time < startTime + dashDuration)
        {
            float progress = (Time.time - startTime) / dashDuration;
            
            // Thành phần tiến về phía trước (giảm tốc độ)
            Vector3 forwardMovement = transform.up * dashSpeed * Time.deltaTime;
            
            // Tính toán vị trí theo vòng cung (giảm phạm vi)
            float currentAngle = Mathf.Lerp(0, dashArcAngle * Mathf.Deg2Rad, progress);
            float radius = dashSpeed * dashDuration / dashArcIntensity;
            
            Vector3 arcOffset = new Vector3(
                -Mathf.Sin(currentAngle) * radius * Time.deltaTime,
                0,
                0
            );
            
            Vector3 rotatedOffset = Quaternion.Euler(0, 0, transform.eulerAngles.z) * arcOffset;
            
            // Di chuyển đơn giản
            transform.position += forwardMovement + rotatedOffset;
            
            // Xoay sprite
            for (int i = 0; i < rotationSteps; i++)
            {
                float stepThreshold = (float)(i + 1) / rotationSteps;
                if (progress >= stepThreshold && !rotationCompleted[i])
                {
                    float stepRotation = dashArcAngle / rotationSteps;
                    transform.Rotate(0, 0, stepRotation);
                    rotationCompleted[i] = true;
                }
            }
            
            yield return null;
        }

        isDashing = false;
    }

    private void AutoMoveForward()
    {   
        // always moves forward in the facing direction
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void ChangeDirection()
    {   
        // rotate a small angle when press LEFT_ARROW key
        transform.Rotate(0, 0, rotationAngle);
    }
}
