Shader "Hidden/SimpleCameraEffects"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _VignetteIntensity ("Vignette Intensity", Range(0,1)) = 0.4
        _ScanlineIntensity ("Scanline Intensity", Range(0,1)) = 0.15
        _ScanlineFrequency ("Scanline Frequency", Float) = 800
        _ChromaticAberration ("Chromatic Aberration", Range(0,0.02)) = 0.004
        _GrainIntensity ("Grain Intensity", Range(0,1)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _TintColor;
            float _VignetteIntensity;
            float _ScanlineIntensity;
            float _ScanlineFrequency;
            float _ChromaticAberration;
            float _GrainIntensity;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 SampleChromatic(sampler2D tex, float2 uv, float2 offset)
            {
                fixed4 c;
                c.r = tex2D(tex, uv + offset).r;
                c.g = tex2D(tex, uv).g;
                c.b = tex2D(tex, uv - offset).b;
                c.a = tex2D(tex, uv).a;
                return c;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // chromatic offset based on uv distance from center
                float2 center = float2(0.5, 0.5);
                float2 dir = uv - center;
                float dist = length(dir);

                float2 caOffset = normalize(dir + 1e-6) * _ChromaticAberration;

                fixed4 col = SampleChromatic(_MainTex, uv, caOffset);

                // vignette
                float vignette = smoothstep(0.0, 1.0, dist);
                vignette = lerp(1.0, 1.0 - vignette, _VignetteIntensity);
                col.rgb *= vignette;

                // scanlines
                float scan = sin(uv.y * _ScanlineFrequency) * _ScanlineIntensity;
                col.rgb -= scan;

                // grain (cheap random based on UV)
                float grain = (frac(sin(dot(uv.xy, float2(12.9898,78.233))) * 43758.5453) - 0.5) * _GrainIntensity;
                col.rgb += grain;

                // tint
                col.rgb *= _TintColor.rgb;

                return saturate(col);
            }
            ENDCG
        }
    }
    FallBack Off
}
