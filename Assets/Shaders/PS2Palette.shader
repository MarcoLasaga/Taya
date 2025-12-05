Shader "Hidden/PS2Palette"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Levels ("Color Levels", Float) = 16
        _ScanlineIntensity ("Scanline Intensity", Range(0,1)) = 0.25
        _Saturation ("Saturation", Range(0,2)) = 0.9
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Levels;
            float _ScanlineIntensity;
            float _Saturation;

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = tex2D(_MainTex, uv);

                // slightly reduce saturation like older hardware
                float gray = dot(col.rgb, float3(0.299,0.587,0.114));
                col.rgb = lerp(float3(gray, gray, gray), col.rgb, _Saturation);

                // ordered-ish dithering using a cheap pseudorandom value
                float d = rand(uv * _ScreenParams.xy);
                // color quantization with dithering
                col.rgb = floor((col.rgb * _Levels) + d) / _Levels;

                // scanlines
                float scan = sin(uv.y * _ScreenParams.y * 1.0) * 0.5 + 0.5;
                col.rgb *= lerp(1.0, scan, _ScanlineIntensity);

                return col;
            }

            ENDCG
        }
    }
    Fallback Off
}
