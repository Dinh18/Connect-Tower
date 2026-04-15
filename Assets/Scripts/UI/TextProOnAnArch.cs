using UnityEngine;
using TMPro;

namespace ntw.CurvedTextMeshPro
{
    /// <summary>
    /// Class uốn cong TextMeshPro theo hình Vòm (Parabol đối xứng) - Chuẩn cho UI Banner
    /// </summary>
    [ExecuteInEditMode]
    public class TextProOnAnArch : TextProOnACurve
    {
        [SerializeField]
        [Tooltip("Độ cong của vòm. Dùng SỐ RẤT NHỎ (VD: 0.001 đến 0.005) vì tính theo Pixel.")]
        private float m_curveMultiplier = 0.002f;

        private float m_oldCurveMultiplier = float.MaxValue;

        protected override bool ParametersHaveChanged()
        {
            bool retVal = m_curveMultiplier != m_oldCurveMultiplier;
            m_oldCurveMultiplier = m_curveMultiplier;
            return retVal;
        }

        protected override Matrix4x4 ComputeTransformationMatrix(Vector3 charMidBaselinePos, float zeroToOnePos, TMP_TextInfo textInfo, int charIdx)
        {
            // 1. Tìm tâm X thực tế của toàn bộ dòng chữ (An toàn, không phụ thuộc vào Alignment)
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
            float centerX = (minX + maxX) / 2f;

            // 2. Khoảng cách thực tế từ ký tự hiện tại đến tâm X (tính bằng Pixel Canvas)
            float realX = charMidBaselinePos.x - centerX;

            // 3. TÍNH VỊ TRÍ Y (Phương trình Parabol: Y = -a * X^2)
            // Vì realX có thể là 100, 200 pixel, m_curveMultiplier phải rất nhỏ (VD: 0.002)
            float yOffset = -m_curveMultiplier * (realX * realX);
            Vector2 newPos = new Vector2(charMidBaselinePos.x, charMidBaselinePos.y + yOffset);

            // 4. TÍNH GÓC XOAY CHUẨN (Đạo hàm: Y' = -2 * a * X)
            float tangent = -2f * m_curveMultiplier * realX;
            float angle = Mathf.Atan(tangent) * Mathf.Rad2Deg;

            // 5. TRẢ VỀ MA TRẬN BIẾN ĐỔI
            return Matrix4x4.TRS(
                new Vector3(newPos.x, newPos.y, 0), 
                Quaternion.AngleAxis(angle, Vector3.forward), 
                Vector3.one
            );
        }
    }
}