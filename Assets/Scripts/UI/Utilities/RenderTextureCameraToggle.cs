using UnityEngine;

public class RenderTextureCameraToggle : MonoBehaviour
{
    [Tooltip("The Camera that renders to the Render Texture")]
    public Camera renderCamera;
    // void Awake()
    // {
    //     if (renderCamera != null)
    //     {
    //         if(!this.gameObject.activeSelf) renderCamera.gameObject.SetActive(false);
    //     } 
    // }

    void OnEnable()
    {
        if (renderCamera != null)
        {
            renderCamera.enabled = true;
        }
    }

    void OnDisable()
    {
        if (renderCamera != null)
        {
            renderCamera.enabled = false;
        }
    }
}
