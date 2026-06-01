Shader "UI/TransparentHole"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Background Color", Color) = (0,0,0,0.8)
        _HoleCenter ("Hole Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleSize ("Hole Size", Vector) = (0.1, 0.1, 0, 0)
        _Softness ("Edge Softness", Float) = 0.05
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 1.0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _HoleCenter;
            float4 _HoleSize;
            float _Softness;
            float _AspectRatio;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float2 center = _HoleCenter.xy;
                
                // Đưa trục x về cùng hệ quy chiếu với trục y
                uv.x *= _AspectRatio;
                center.x *= _AspectRatio;

                // Tính khoảng cách tương đối tới tâm theo 2 trục của elip
                float2 diff = uv - center;
                float dist = length(float2(diff.x / max(0.001, _HoleSize.x), diff.y / max(0.001, _HoleSize.y)));

                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Tạo hiệu ứng mờ dần (softness)
                // dist = 1.0 là biên của elip. Tính toán độ mềm (softness) tương đối.
                float normSoftness = _Softness / max(0.001, min(_HoleSize.x, _HoleSize.y));
                float alphaMult = smoothstep(1.0, 1.0 + normSoftness, dist);
                c.a *= alphaMult;

                return c;
            }
            ENDCG
        }
    }
}