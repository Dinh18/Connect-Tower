Shader "UI/ImageBlendEffect"
{
    Properties
    {
        // Các thuộc tính bắt buộc của UI
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Các thuộc tính của Frost Effect
        _BlendTex ("Frost Texture (RGB)", 2D) = "white" {}
        _BumpMap ("Frost Normalmap", 2D) = "bump" {}

        // --- CÁC THUỘC TÍNH ẨN (ĐƯỢC C# SCRIPT ĐIỀU KHIỂN) ---
        [HideInInspector] _BlendAmount ("Blend Amount", Float) = 0
        [HideInInspector] _EdgeSharpness ("Edge Sharpness", Float) = 1
        [HideInInspector] _SeeThroughness ("See Throughness", Float) = 0.2
        [HideInInspector] _Distortion ("Distortion", Float) = 0.1

        // --- HỖ TRỢ UI MASK ---
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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

        // Khối lệnh giúp Shader này bị cắt gọn gàng nếu nằm trong Mask
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
        Blend SrcAlpha OneMinusSrcAlpha // Bật chế độ trong suốt cho UI
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            sampler2D _BlendTex;
            sampler2D _BumpMap;

            float _BlendAmount;
            float _EdgeSharpness;
            float _SeeThroughness;
            float _Distortion;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {
                // 1. Tính toán vùng lan rộng của băng
                float4 frostColor = tex2D(_BlendTex, IN.texcoord);
                float frostAlpha = frostColor.a + (_BlendAmount * 2 - 1);
                frostAlpha = saturate(frostAlpha * _EdgeSharpness - (_EdgeSharpness - 1) * 0.5);

                // 2. Làm méo ảnh (Distortion)
                half2 bump = UnpackNormal(tex2D(_BumpMap, IN.texcoord)).rg;
                // Áp dụng độ méo lên UV của chính UI Sprite
                float2 distortedUV = IN.texcoord + bump * frostAlpha * _Distortion;

                // 3. Lấy màu gốc của UI Image (Viền Booster)
                half4 spriteColor = tex2D(_MainTex, distortedUV) * IN.color;

                // 4. Tính toán độ hòa trộn (Seethrough / Overlay)
                float4 overlayColor = frostColor;
                overlayColor.rgb = spriteColor.rgb * (frostColor.rgb + 0.5) * (frostColor.rgb + 0.5);
                frostColor = lerp(frostColor, overlayColor, _SeeThroughness);

                // 5. Gộp lại: Chỉ hiện băng ở những nơi có hình ảnh viền (spriteColor.a)
                half3 finalRGB = lerp(spriteColor.rgb, frostColor.rgb, frostAlpha);
                
                // GIỮ LẠI ĐỘ TRONG SUỐT CỦA VIỀN GỐC (Tránh băng lan ra ngoài viền)
                return half4(finalRGB, spriteColor.a);
            }
            ENDCG
        }
    }
}