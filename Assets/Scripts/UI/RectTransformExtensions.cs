using UnityEngine;

public static class RectTransformExtensions
{
    // Đổi Pivot nhưng không làm UI bị nhảy vị trí trên màn hình
    public static void SetPivotWithoutMoving(this RectTransform rectTransform, Vector2 pivot)
    {
        if (rectTransform == null) return;

        // Lưu vị trí các góc trong không gian thế giới trước khi đổi pivot
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // Đổi pivot
        rectTransform.pivot = pivot;

        // Lấy lại vị trí các góc sau khi đổi pivot
        Vector3[] newCorners = new Vector3[4];
        rectTransform.GetWorldCorners(newCorners);

        // Tính toán độ lệch và dịch chuyển rectTransform về vị trí cũ
        Vector3 offset = corners[0] - newCorners[0];
        rectTransform.position += offset;
    }
}