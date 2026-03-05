Shader "Custom/SelectionRingURP"
{
    Properties
    {
        _Color ("Color", Color) = (0, 1, 0, 1)
        _Inner ("Inner Radius (0-0.5)", Range(0,0.5)) = 0.42
        _Outer ("Outer Radius (0-0.5)", Range(0,0.5)) = 0.48
        _Soft  ("Edge Softness", Range(0.0001,0.1)) = 0.01

        _PulseSpeed ("Pulse Speed", Range(0,10)) = 0
        _PulseAmp   ("Pulse Amount", Range(0,0.25)) = 0

        _DashCount  ("Dash Count (0=off)", Range(0,64)) = 0
        _DashSharp  ("Dash Sharpness", Range(1,20)) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "SelectionRing"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Inner;
                float _Outer;
                float _Soft;
                float _PulseSpeed;
                float _PulseAmp;
                float _DashCount;
                float _DashSharp;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float ringMask(float2 uv, float inner, float outer, float soft)
            {
                // uv center
                float2 p = uv - 0.5;
                float r = length(p);

                
                float a = smoothstep(inner - soft, inner + soft, r);
                float b = 1.0 - smoothstep(outer - soft, outer + soft, r);
                return saturate(a * b);
            }

            float dashMask(float2 uv, float dashCount, float sharp)
            {
                if (dashCount <= 0.5) return 1.0;

                float2 p = uv - 0.5;
                float ang = atan2(p.y, p.x);     // -pi to pi
                float t = (ang + PI) / (2.0 * PI); 

                // sin-based dash pattern
                float s = sin(t * dashCount * 2.0 * PI);
                // sharpen
                return pow(saturate((s * 0.5 + 0.5)), sharp);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y;

                float pulse = 1.0;
                if (_PulseSpeed > 0.001 && _PulseAmp > 0.001)
                    pulse = 1.0 + sin(t * _PulseSpeed) * _PulseAmp;

                float inner = _Inner * pulse;
                float outer = _Outer * pulse;

                float m = ringMask(IN.uv, inner, outer, _Soft);
                float d = dashMask(IN.uv, _DashCount, _DashSharp);

                half4 col = _Color;
                col.a *= (m * d);

                return col;
            }
            ENDHLSL
        }
    }
}
