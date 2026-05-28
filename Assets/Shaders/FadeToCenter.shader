Shader "Custom/UI/FadeToCenter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0, 0, 0, 0.8) // Màu nền tối mặc định (Alpha 0.8)
        
        // _Center: Toạ độ tâm của lỗ sáng (chuẩn hoá 0-1)
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0) 
        
        // _Radius: Bán kính của lỗ sáng (khoảng cách bắt đầu mờ)
        _Radius ("Radius", Range(0, 2)) = 0.1 
        
        // _Softness: Độ mềm của viền lỗ sáng
        _Softness ("Softness", Range(0.01, 2)) = 0.1 
        
        // _Fade: Thông số từ 0 - 1 để kiểm soát độ đậm nhạt tổng thể
        _Fade ("Fade Amount", Range(0, 1)) = 1.0 

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

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
            float4 _Center;
            float _Radius;
            float _Softness;
            float _Fade;

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
                fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Điều chỉnh tỉ lệ màn hình (nếu ảnh bị stretch không vuông, lỗ bị dẹp)
                // Tạm thời dùng khoảng cách UV thông thường
                float dist = distance(IN.texcoord, _Center.xy);
                
                // Ở xa tâm (dist > Radius), mask = 1 (Tối hoàn toàn theo _Color.a)
                // Càng gần tâm (dist < Radius - Softness), mask = 0 (Trong suốt, lủng lỗ sáng)
                float darknessMask = smoothstep(_Radius - _Softness, _Radius, dist);
                
                // Alpha cuối cùng = Alpha gốc * Mask * Fade
                col.a *= darknessMask * _Fade;
                
                return col;
            }
            ENDCG
        }
    }
}
