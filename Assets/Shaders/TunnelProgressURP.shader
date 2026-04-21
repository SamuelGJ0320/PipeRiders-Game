Shader "Custom/TunnelProgressURP"
{
    Properties
    {
        _MetersPerColor ("Metros Por Color", Range(1,30)) = 10
        _TransitionMeters ("Metros De Transicion", Range(0,10)) = 0
        _SegmentStartMeters ("Segmento Inicio (m)", Float) = 0
        _SegmentLengthMeters ("Segmento Longitud (m)", Float) = 10
        _PaletteColor0 ("Color Paleta 0", Color) = (1,0,0,1)
        _PaletteColor1 ("Color Paleta 1", Color) = (1,1,0,1)
        _PaletteColor2 ("Color Paleta 2", Color) = (0,1,0,1)
        _PaletteColor3 ("Color Paleta 3", Color) = (0,1,1,1)
        _PaletteColor4 ("Color Paleta 4", Color) = (0,0.2,1,1)
        _PaletteColor5 ("Color Paleta 5", Color) = (0.65,0,1,1)
        _PaletteColor6 ("Color Paleta 6", Color) = (1,0,0.75,1)
        _LaneCount ("Lane Count", Range(2,12)) = 8
        _LaneLineWidth ("Lane Line Width", Range(0.002,0.08)) = 0.02
        _LaneLineColor ("Lane Line Color", Color) = (0,0,0,1)
        _LaneLineStrength ("Lane Line Strength", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _MetersPerColor;
                float _TransitionMeters;
                float _SegmentStartMeters;
                float _SegmentLengthMeters;
                float4 _PaletteColor0;
                float4 _PaletteColor1;
                float4 _PaletteColor2;
                float4 _PaletteColor3;
                float4 _PaletteColor4;
                float4 _PaletteColor5;
                float4 _PaletteColor6;
                float _LaneCount;
                float _LaneLineWidth;
                float4 _LaneLineColor;
                float _LaneLineStrength;
            CBUFFER_END

            float3 PaletteColorByIndex(int idx)
            {
                if (idx <= 0) return _PaletteColor0.rgb;
                if (idx == 1) return _PaletteColor1.rgb;
                if (idx == 2) return _PaletteColor2.rgb;
                if (idx == 3) return _PaletteColor3.rgb;
                if (idx == 4) return _PaletteColor4.rgb;
                if (idx == 5) return _PaletteColor5.rgb;
                return _PaletteColor6.rgb;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float metersPerColor = max(0.1, _MetersPerColor);
                float transitionMeters = clamp(_TransitionMeters, 0.0, metersPerColor);
                float transitionFrac = saturate(transitionMeters / metersPerColor);

                // Distancia global en metros a lo largo del tunel (sin repetir paleta).
                float meters = max(0.0, _SegmentStartMeters + saturate(IN.uv.y) * _SegmentLengthMeters);
                float phase = meters / metersPerColor;
                int idx = (int)floor(phase);
                float local = frac(phase);

                const int activeColors = 7;
                int idxA = clamp(idx, 0, activeColors - 1);
                int idxB = clamp(idx + 1, 0, activeColors - 1);

                half3 colorA = (half3)PaletteColorByIndex(idxA);
                half3 colorB = (half3)PaletteColorByIndex(idxB);

                // Si transition = 0, el bloque es solido (sin mezcla).
                float blend = 0.0;
                if (transitionFrac > 0.0001)
                {
                    float blendStart = 1.0 - transitionFrac;
                    blend = smoothstep(blendStart, 1.0, local);
                }
                half3 rainbow = lerp(colorA, colorB, blend);

                float laneCount = max(2.0, _LaneCount);
                // Delimitadores por angulo real alrededor del tunel, independiente del UV.
                float angle = atan2(IN.positionOS.z, IN.positionOS.x);
                float angle01 = frac((angle + PI) / (2.0 * PI));
                float lanePos = frac(angle01 * laneCount);
                float distToBoundary = min(lanePos, 1.0 - lanePos);
                float laneLineMask = 1.0 - smoothstep(0.0, _LaneLineWidth, distToBoundary);

                half3 colorOut = lerp(rainbow, (half3)_LaneLineColor.rgb, laneLineMask * _LaneLineStrength);
                return half4(colorOut, 1.0h);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
