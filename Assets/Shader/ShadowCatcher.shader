Shader "Custom/URP_ShadowCatcher"
{
    Properties
    {
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 0.5)
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Các từ khóa bắt buộc để Unity tính toán bóng đổ trong URP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            // Gọi thư viện của URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ShadowColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Chuyển đổi tọa độ Local sang World và Clip Space
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Lấy tọa độ đổ bóng tại vị trí pixel này
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                // Lấy giá trị ánh sáng (1 = có sáng, 0 = bị che bóng hoàn toàn)
                half shadowAttenuation = MainLightRealtimeShadow(shadowCoord);

                // Tính toán Alpha: 
                // Chỗ nào ánh sáng bị che (shadowAttenuation thấp) thì hiện màu đậm.
                // Chỗ nào ánh sáng chiếu tới (shadowAttenuation = 1) thì alpha = 0 (tàng hình).
                half alpha = (1.0 - shadowAttenuation) * _ShadowColor.a;

                return half4(_ShadowColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}