using UnityEngine;
using TMPro;

namespace ntw.CurvedTextMeshPro
{
    public interface ITMPVertexModifier
    {
        bool NeedsUpdate();
        void ModifyVertices(TMP_TextInfo textInfo);
    }
}

[RequireComponent(typeof(TMP_Text))]
[ExecuteInEditMode]
public class TMPWavyText : MonoBehaviour, ntw.CurvedTextMeshPro.ITMPVertexModifier
{
    [Header("Idle Wave Settings (Sóng liên tục)")]
    public bool enableWave = true;
    public float waveSpeed = 3f;
    public float waveHeight = 5f;
    public float waveFrequency = 1f;

    [Header("Show Animation (Gợn sóng xuất hiện)")]
    [Range(0f, 1f)]
    public float showProgress = 1f;
    public float showSpread = 0.8f;
    public float showOvershoot = 1.70158f;

    private TMP_Text m_TextComponent;
    private bool m_hasCurveScript;

    private void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
        m_hasCurveScript = GetComponent<ntw.CurvedTextMeshPro.TextProOnACurve>() != null;
    }

    private void Update()
    {
        // Nếu object có script TextProOnACurve, script đó sẽ tự thu thập các ITMPVertexModifier 
        // và tự gọi hàm ModifyVertices sau khi đã uốn cong xong. Ta không tự gọi ForceMeshUpdate.
        if (m_hasCurveScript) return;

        if (!NeedsUpdate()) return;

        m_TextComponent.ForceMeshUpdate();
        ModifyVertices(m_TextComponent.textInfo);
        m_TextComponent.UpdateVertexData();
    }

    public bool NeedsUpdate()
    {
        if (!enableWave && showProgress >= 1f) return false;
        return true; 
    }

    public void ModifyVertices(TMP_TextInfo textInfo)
    {
        if (textInfo == null || textInfo.characterCount == 0) return;

        int totalChars = Mathf.Max(1, textInfo.characterCount);

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            float charNorm = (float)i / totalChars;
            float animY = 0f;
            float animScale = 1f;
            float animAlpha = 1f;

            // 1. Show Animation
            if (showProgress < 1f)
            {
                float localProgress = (showProgress * (1f + showSpread) - charNorm * showSpread);
                localProgress = Mathf.Clamp01(localProgress);
                animAlpha = localProgress;

                if (localProgress <= 0f)
                {
                    animScale = 0f;
                }
                else if (localProgress >= 1f)
                {
                    animScale = 1f;
                }
                else
                {
                    float t = localProgress - 1f;
                    animScale = (t * t * ((showOvershoot + 1f) * t + showOvershoot) + 1f);
                }

                animY += (1f - localProgress) * -40f; 
            }

            // 2. Idle Wave
            if (enableWave)
            {
                float wave = Mathf.Sin(Time.time * waveSpeed - charNorm * waveFrequency * Mathf.PI * 2f);
                animY += wave * waveHeight * showProgress; 
            }

            // Apply to vertices
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

            // Tính tâm ký tự để scale
            Vector3 charMid = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2f;

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = vertices[vertexIndex + j];
                
                // Scale
                if (animScale != 1f)
                {
                    v = charMid + (v - charMid) * animScale;
                }
                
                // Translate
                v.y += animY;

                vertices[vertexIndex + j] = v;

                // Alpha Fade
                if (animAlpha < 1f)
                {
                    Color32 c = colors[vertexIndex + j];
                    c.a = (byte)(c.a * animAlpha);
                    colors[vertexIndex + j] = c;
                }
            }
        }
    }
}
