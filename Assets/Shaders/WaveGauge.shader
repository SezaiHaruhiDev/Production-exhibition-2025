Shader "Custom/UI/WaveGauge"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _WaveAmp ("Wave Amplitude", Range(0, 0.1)) = 0.02
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 2.0
        _WaveFreq ("Wave Frequency", Range(0, 50)) = 3.0
        _GlowPower ("Glow Power", Range(0, 5)) = 1.0
        
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
            Name "Default"
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            float _FillAmount;
            float _WaveAmp;
            float _WaveSpeed;
            float _WaveFreq;
            float _GlowPower;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // UI Default Texture Sample
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                // --- Wave Gauge Logic (Vertical / Liquid Cup) ---
                
                // Vertical Fill Position (0 at bottom, 1 at top)
                float fillPos = IN.texcoord.y;
                
                // Wave depends on X (horizontal position)
                // wave = sin(x * freq + time * speed) * amp
                float wave = sin(IN.texcoord.x * _WaveFreq * 6.28 + _Time.y * _WaveSpeed) * _WaveAmp;
                
                // Effective Fill Threshold
                float threshold = _FillAmount + wave;
                
                // Alpha Mask (Below liquid level is visible)
                float alphaMask = step(fillPos, threshold);
                
                // Glow Band logic
                // Provide brightness near the edge
                float edgeWidth = 0.05; // 5% width
                float dist = abs(fillPos - threshold);
                float glow = 0;
                if (dist < edgeWidth && fillPos <= threshold)
                {
                   glow = 1.0 - (dist / edgeWidth);
                   glow = pow(glow, 2); // sharpen
                }
                
                // Apply Glow Power
                glow *= _GlowPower;
                
                // Combine
                color.a *= alphaMask;
                color.rgb += glow * _Color.rgb; // Additive glow on top

                return color;
            }
            ENDCG
        }
    }
}
