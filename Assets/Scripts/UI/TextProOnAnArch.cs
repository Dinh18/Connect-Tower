using UnityEngine;
using TMPro;

namespace ntw.CurvedTextMeshPro
{
    /// <summary>
    /// Class uốn cong TextMeshPro theo hình Vòm - Chuẩn cho UI Banner (Sử dụng đường tròn để giữ nguyên khoảng cách chữ)
    /// </summary>
    [ExecuteInEditMode]
    public class TextProOnAnArch : TextProOnACurve
    {
        [SerializeField]
        [Tooltip("Độ cong của vòm. Dùng SỐ RẤT NHỎ (VD: 0.001 đến 0.005) vì tính theo Pixel.")]
        private float m_curveMultiplier = 0.002f;

        [SerializeField]
        [Tooltip("Nếu bật, đỉnh vòm sẽ luôn nằm ở chính giữa khung UI (X = 0). Rất quan trọng cho UI Banner để đỉnh text không bị lệch so với hình nền.")]
        private bool m_centerOnRect = true;

        private float m_oldCurveMultiplier = float.MaxValue;
        private bool m_oldCenterOnRect = false;

        protected override bool ParametersHaveChanged()
        {
            bool retVal = m_curveMultiplier != m_oldCurveMultiplier || m_centerOnRect != m_oldCenterOnRect;
            m_oldCurveMultiplier = m_curveMultiplier;
            m_oldCenterOnRect = m_centerOnRect;
            return retVal;
        }

        protected override Matrix4x4 ComputeTransformationMatrix(Vector3 charMidBaselinePos, float zeroToOnePos, TMP_TextInfo textInfo, int charIdx)
        {
            float centerX = 0f;

            if (!m_centerOnRect)
            {
                // 1. Tìm tâm X thực tế của toàn bộ dòng chữ dựa trên các ký tự hiển thị
                float minX = float.MaxValue;
                float maxX = float.MinValue;
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    if (textInfo.characterInfo[i].isVisible)
                    {
                        minX = Mathf.Min(minX, textInfo.characterInfo[i].bottomLeft.x);
                        maxX = Mathf.Max(maxX, textInfo.characterInfo[i].bottomRight.x);
                    }
                }
                centerX = (minX + maxX) / 2f;
            }

            // 2. Khoảng cách thực tế từ ký tự hiện tại đến tâm vòm (tính bằng Pixel Canvas)
            float realX = charMidBaselinePos.x - centerX;

            // 3. TÍNH VỊ TRÍ VÀ GÓC XOAY
            Vector2 newPos = charMidBaselinePos;
            float angle = 0f;

            if (Mathf.Abs(m_curveMultiplier) > 0.00001f)
            {
                float R = 1f / (2f * m_curveMultiplier);
                
                // Chiều dài cung tròn (arc length) chính là realX
                float theta = realX / R; 

                // Tính toạ độ mới tương đối với tâm uốn cong
                float newX = centerX + R * Mathf.Sin(theta);
                float newY = charMidBaselinePos.y + R * Mathf.Cos(theta) - R;

                newPos = new Vector2(newX, newY);
                angle = -theta * Mathf.Rad2Deg;
            }

            // 4. TRẢ VỀ MA TRẬN BIẾN ĐỔI
            return Matrix4x4.TRS(
                new Vector3(newPos.x, newPos.y, 0), 
                Quaternion.AngleAxis(angle, Vector3.forward), 
                Vector3.one
            );
        }
    }
}