using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class BottomSafeArea : MonoBehaviour
{
    private RectTransform rectTransform;


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplyBottomSafeArea();
    }

    void ApplyBottomSafeArea()
    {
        // Lấy thông số vùng an toàn của máy
        Rect safeArea = Screen.safeArea;

        // Tính tỉ lệ phần cằm so với tổng chiều cao màn hình (ví dụ: cằm chiếm 5% = 0.05)
        float bottomRatio = safeArea.y / Screen.height;

        // 1. Cập nhật mỏ neo Bottom (Y min) đẩy lên đúng bằng tỉ lệ cái cằm
        Vector2 newAnchorMin = rectTransform.anchorMin;
        newAnchorMin.y = bottomRatio;
        rectTransform.anchorMin = newAnchorMin;

        // 2. Reset padding ở đáy về 0 để UI bám sát khít vào mỏ neo mới
        Vector2 newOffsetMin = rectTransform.offsetMin;
        newOffsetMin.y = 0;
        rectTransform.offsetMin = newOffsetMin;
    }
}