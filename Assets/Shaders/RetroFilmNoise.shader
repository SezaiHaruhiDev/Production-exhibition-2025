Shader "Custom/RetroFilmNoise"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseInstensity ("Noise Intensity", Range(0, 1)) = 0.5
        _ScratchIntensity ("Scratch Intensity", Range(0, 1)) = 0.5
        _SepiaColor ("Sepia Color", Color) = (1, 0.9, 0.7, 1)
        _VignetteIntensity ("Vignette Intensity", Range(0, 5)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _NoiseInstensity;
            float _ScratchIntensity;
            fixed4 _SepiaColor;
            float _VignetteIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float random (float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _Time.y;
                float seed = dot(i.uv, float2(12.9898, 78.233)) + t;

                float noise = random(float2(seed, seed));

                float scratch = 0;
                if (random(float2(t * 10, 0)) > 0.9)
                {
                    float dist = abs(i.uv.x - random(float2(t, 0)));
                    if (dist < 0.005) scratch = 1;
                }

                fixed4 col = tex2D(_MainTex, i.uv);

                col.rgb -= noise * _NoiseInstensity;
                col.rgb += scratch * _ScratchIntensity;

                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(col.rgb, float3(gray, gray, gray) * _SepiaColor.rgb, 0.8);

                // ビネット（周辺減光）
                float2 distFromCenter = i.uv - 0.5;
                float len = length(distFromCenter);
                float vignette = saturate(1.0 - len * _VignetteIntensity);
                
                col.rgb *= vignette;

                // ビネット部分は不透明に近づける（黒枠を作るため）
                float finalAlpha = saturate(_SepiaColor.a + (1.0 - vignette));

                return fixed4(col.rgb, finalAlpha);
            }
            ENDCG
        }
    }
}