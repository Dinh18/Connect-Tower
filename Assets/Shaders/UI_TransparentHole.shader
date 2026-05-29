Shader "UI/TransparentHole"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Background Color", Color) = (0,0,0,0.8)
        _HoleCenter ("Hole Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.1
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
            float _HoleRadius;
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
                
                // Cân bằng tỉ lệ khung hình để lỗ hổng luôn là hình tròn (không bị méo thành oval)
                uv.x *= _AspectRatio;
                center.x *= _AspectRatio;

                // Tính khoảng cách từ pixel hiện tại tới tâm lỗ hổng
                float dist = distance(uv, center);

                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Tạo hiệu ứng mờ dần (softness/feather) ở viền lỗ hổng thay vì cắt cứng
                // Từ _HoleRadius đến _HoleRadius + _Softness, alpha sẽ tăng dần từ 0 đến 1 (nhân với alpha hiện tại)
                float alphaMult = smoothstep(_HoleRadius, _HoleRadius + _Softness, dist);
                c.a *= alphaMult;

                return c;
            }
            ENDCG
        }
    }
}