using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Graphic))]
public class UIBannerWaveEffect : BaseMeshEffect
{
    [Header("Wave Settings")]
    [Tooltip("Bật tắt hiệu ứng gợn sóng")]
    public bool enableWave = true;

    [Tooltip("Tốc độ gợn sóng (càng lớn càng nhanh)")]
    public float waveSpeed = 3f;

    [Tooltip("Độ cao của gợn sóng (pixel)")]
    public float waveHeight = 10f;

    [Tooltip("Độ nhặt của sóng (càng lớn sóng càng ngắn)")]
    public float waveFrequency = 1f;

    [Header("Mesh Settings")]
    [Tooltip("Số lượng cắt nhỏ lưới để tạo độ cong mượt (Chỉ áp dụng khi Image Type là Simple)")]
    [Range(1, 100)]
    public int subdivisions = 20;

    [Header("Show Animation")]
    [Range(0f, 1f)]
    [Tooltip("Hiệu ứng trải ra từ giữa (0 = thu gọn ở giữa, 1 = mở rộng hoàn toàn)")]
    public float unfoldProgress = 1f;

    private void Update()
    {
        if (graphic != null && (enableWave || unfoldProgress < 1f))
        {
            // Yêu cầu Canvas vẽ lại (cập nhật lưới) mỗi frame để tạo hiệu ứng động
            graphic.SetVerticesDirty();
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        int vertCount = vh.currentVertCount;
        if (vertCount == 0) return;

        // Nếu là Simple Image (4 đỉnh), ta sẽ cắt nhỏ (subdivide) nó ra để uốn cong cho mượt
        if (vertCount == 4)
        {
            UIVertex v0 = new UIVertex(); vh.PopulateUIVertex(ref v0, 0); // Bottom-Left
            UIVertex v1 = new UIVertex(); vh.PopulateUIVertex(ref v1, 1); // Top-Left
            UIVertex v2 = new UIVertex(); vh.PopulateUIVertex(ref v2, 2); // Top-Right
            UIVertex v3 = new UIVertex(); vh.PopulateUIVertex(ref v3, 3); // Bottom-Right

            // Xoá lưới cũ
            vh.Clear();

            int N = subdivisions;
            float segmentWidth = (v3.position.x - v0.position.x) / N;
            float uvWidth = (v3.uv0.x - v0.uv0.x) / N;

            float centerX = (v0.position.x + v3.position.x) / 2f;

            // Tạo các đỉnh mới
            for (int i = 0; i <= N; i++)
            {
                float xPos = v0.position.x + i * segmentWidth;
                float u = v0.uv0.x + i * uvWidth;

                // Tính toán độ lệch Y dựa trên hàm Sin
                float waveOffset = 0f;
                if (enableWave)
                {
                    float normalizedX = (float)i / N;
                    waveOffset = Mathf.Sin(Time.time * waveSpeed - normalizedX * waveFrequency * Mathf.PI * 2f) * waveHeight;
                }

                // Hiệu ứng "Trải ra" (Unfold/Roll out)
                if (unfoldProgress < 1f)
                {
                    xPos = Mathf.Lerp(centerX, xPos, unfoldProgress);
                }

                // Đỉnh dưới
                UIVertex vb = new UIVertex();
                vb.position = new Vector3(xPos, v0.position.y + waveOffset, v0.position.z);
                vb.color = v0.color;
                vb.uv0 = new Vector2(u, v0.uv0.y);
                vh.AddVert(vb);

                // Đỉnh trên
                UIVertex vt = new UIVertex();
                vt.position = new Vector3(xPos, v1.position.y + waveOffset, v1.position.z);
                vt.color = v1.color;
                vt.uv0 = new Vector2(u, v1.uv0.y);
                vh.AddVert(vt);
            }

            // Tạo các tam giác (triangles) nối các đỉnh lại
            for (int i = 0; i < N; i++)
            {
                int startIndex = i * 2;
                // vb = startIndex, vt = startIndex + 1
                // next_vb = startIndex + 2, next_vt = startIndex + 3
                vh.AddTriangle(startIndex + 0, startIndex + 1, startIndex + 3);
                vh.AddTriangle(startIndex + 3, startIndex + 2, startIndex + 0);
            }
        }
        else
        {
            // Nếu không phải Simple Image (ví dụ Sliced Image), ta chỉ uốn các đỉnh có sẵn
            // (Hiệu ứng có thể không mượt nếu Sliced Image quá lớn mà không có đủ lưới)
            List<UIVertex> verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);

            Rect rect = graphic.rectTransform.rect;
            float width = rect.width;
            float minX = rect.xMin;

            vh.Clear();
            
            // Populate lại verts (nhưng GetUIVertexStream trả về tam giác, ta nên dùng PopulateUIVertex để an toàn hơn)
            for (int i = 0; i < vertCount; i++)
            {
                UIVertex v = new UIVertex();
                vh.PopulateUIVertex(ref v, i);

                float normalizedX = (v.position.x - minX) / width;
                float waveOffset = Mathf.Sin(Time.time * waveSpeed - normalizedX * waveFrequency * Mathf.PI * 2f) * waveHeight;

                v.position.y += waveOffset;
                vh.AddVert(v);
            }

            // Phục hồi lại triangles cũ
            for (int i = 0; i < vh.currentVertCount; i += 3)
            {
                vh.AddTriangle(i, i + 1, i + 2);
            }
        }
    }
}
