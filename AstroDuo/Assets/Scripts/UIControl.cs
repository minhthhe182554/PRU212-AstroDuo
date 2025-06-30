using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControl : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] 
    private Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f); // Image scale 1.2x when hover

    public void ToSetting()
    {
        SceneManager.LoadScene(GameConst.SETTING_SCENE); 
    }

    public void ToSelectSkin()
    {
        SceneManager.LoadScene(GameConst.SELECT_SKIN_SCENE);
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
}
