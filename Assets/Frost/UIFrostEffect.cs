using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))] // Đảm bảo object gắn script này phải có component Image
[AddComponentMenu("UI Effects/UI Frost")]
public class UIFrostEffect : MonoBehaviour
{
    [Header("Frost Settings")]
    [Range(0f, 1f)] public float FrostAmount = 0.5f; // 0 = Không đóng băng, 1 = Đóng băng hoàn toàn
    public float EdgeSharpness = 1f;
    [Range(0f, 1f)] public float minFrost = 0f;
    [Range(0f, 1f)] public float maxFrost = 1f;
    [Range(0f, 1f)] public float seethroughness = 0.2f;
    public float distortion = 0.1f;

    private Image uiImage;
    private Material uiMaterialInstance;

    private void Awake()
    {
        uiImage = GetComponent<Image>();

        // Tạo ra một bản sao (Instance) của Material để khi ta thay đổi thông số, 
        // nó không làm ảnh hưởng đến các UI khác đang dùng chung Material gốc.
        if (uiImage.material != null)
        {
            uiMaterialInstance = new Material(uiImage.material);
            uiImage.material = uiMaterialInstance;
        }
        else
        {
            Debug.LogWarning("UI Image chưa được gán Frost Material!");
        }
    }

    // Update dùng để bạn có thể kéo thanh FrostAmount trong Inspector và xem trực tiếp
    private void Update()
    {
        UpdateShaderProperties();
    }

    private void UpdateShaderProperties()
    {
        if (uiMaterialInstance != null)
        {
            // Truyền các thông số từ C# vào Shader của Material
            float finalBlend = Mathf.Clamp01(Mathf.Clamp01(FrostAmount) * (maxFrost - minFrost) + minFrost);
            uiMaterialInstance.SetFloat("_BlendAmount", finalBlend);
            uiMaterialInstance.SetFloat("_EdgeSharpness", EdgeSharpness);
            uiMaterialInstance.SetFloat("_SeeThroughness", seethroughness);
            uiMaterialInstance.SetFloat("_Distortion", distortion);
        }
    }

    private void OnDestroy()
    {
        // Phải xóa Material ảo khi object bị hủy để tránh rò rỉ bộ nhớ (Memory Leak)
        if (uiMaterialInstance != null)
        {
            Destroy(uiMaterialInstance);
        }
    }
}