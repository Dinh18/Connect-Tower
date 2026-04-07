using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Chuyển đổi tọa độ Pixel của Safe Area thành tọa độ Anchor tỉ lệ (0 đến 1)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Áp dụng viền an toàn cho Panel
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}