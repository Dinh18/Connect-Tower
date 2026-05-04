Shader "Custom/UI_CountdownRadialOpen"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Progress ("Open Amount (0-1)", Range(0, 1)) = 0.0 
        _GreyOverlayAmount ("Grey Overlay Amount", Range(0, 1)) = 0.5 
        _Smoothness ("Smoothness", Range(0, 0.1)) = 0.005 

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            sampler2D _MainTex;

            float _Progress;
            float _GreyOverlayAmount;
            float _Smoothness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 col = (tex2D(_MainTex, input.texcoord) + _TextureSampleAdd) * input.color;

                float2 centeredUV = input.texcoord - 0.5;
                float angle = atan2(centeredUV.x, centeredUV.y);
                float finalAngle = (angle < 0.0) ? (angle + 6.28318) : angle;
                finalAngle /= 6.28318;

                // ĐÃ FIX: Viết đúng chuẩn smoothstep(min, max, x) của D3D11
                float mask = 1.0 - smoothstep(_Progress - _Smoothness, _Progress + _Smoothness, finalAngle);

                fixed greyValue = dot(col.rgb, fixed3(0.299, 0.587, 0.114));
                fixed4 greyCol = fixed4(greyValue, greyValue, greyValue, col.a);

                fixed4 closedCol = lerp(col, greyCol, _GreyOverlayAmount);
                fixed4 finalColor = lerp(closedCol, col, mask);

                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif
                finalColor.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);

                return finalColor;
            }
            ENDCG
        }
    }
}