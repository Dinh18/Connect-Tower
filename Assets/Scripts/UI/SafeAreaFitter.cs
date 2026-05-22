using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rectTransform;
    void Awake()
    {
        ApplySafeArea();    
    }

    void ApplySafeArea()
    {
        rectTransform = GetComponent<RectTransform>();
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

        // rectTransform.offsetMin = Vector2.zero;
        // rectTransform.offsetMax = Vector2.zero;
    }

    #if UNITY_EDITOR
    // Hàm này vẽ một khung viền màu xanh lá quanh Safe Area trong tab Scene
    private void OnDrawGizmos()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            // Lấy tọa độ 4 góc của RectTransform trong không gian thế giới (World Space)
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            // Đặt màu cho viền vẽ (Bạn có thể đổi sang Color.red, Color.yellow... tùy ý)
            Gizmos.color = Color.green;

            // Nối 4 góc lại thành 1 hình chữ nhật
            Gizmos.DrawLine(corners[0], corners[1]); // Dưới trái -> Trên trái
            Gizmos.DrawLine(corners[1], corners[2]); // Trên trái -> Trên phải
            Gizmos.DrawLine(corners[2], corners[3]); // Trên phải -> Dưới phải
            Gizmos.DrawLine(corners[3], corners[0]); // Dưới phải -> Dưới trái
        }
    }
    #endif
}

